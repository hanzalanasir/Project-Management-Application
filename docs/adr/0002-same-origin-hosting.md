# ADR-0002: Same-origin hosting — the API serves the Angular app

**Status**: Accepted · **Date**: 2026-07-22 · **Relates to**: Constitution V.6, XI.1; feature 001 (refresh cookie)

## Context

Constitution XI.1 permits the built Angular app to be served either as IIS static files or by the
API's static-file middleware. That choice is not cosmetic: feature 001 delivers the refresh token as
an httpOnly cookie, and the cookie's `SameSite` value — and whether CORS needs credentialed
cross-origin support at all — follows directly from the origin topology.

## Decision

**One origin, one IIS site.** The .NET API serves the production Angular bundle via static-file
middleware with SPA fallback routing; `/api/*` is handled by the API.

- The refresh cookie uses **`SameSite=Strict; HttpOnly; Secure; Path=/api/auth`** — the strongest setting.
- **Local development mirrors production**: `ng serve` proxies `/api` to the API via `proxy.conf.json`,
  so the browser sees a single origin in dev too. `SameSite=Strict` therefore works in every environment
  and never has to be weakened for local work.
- CORS keeps its explicit allow-list (Constitution V.6) but is effectively inert in production; no
  wildcard is used anywhere.

## Alternatives rejected

**Separate origins** (IIS static site + separate API site) would force `SameSite=None; Secure` on the
refresh cookie, a credentialed CORS configuration, and a weaker CSRF posture — all to gain deployment
independence this single-server IIS target does not need.

## Consequences

Deployment is a single publish + one IIS site (simpler for the brief's deliverable). Splitting the
frontend onto a CDN or separate host later is a breaking change to the cookie policy and would need a
new ADR. The dev-server proxy is mandatory, not optional — without it, dev breaks on the cookie.
