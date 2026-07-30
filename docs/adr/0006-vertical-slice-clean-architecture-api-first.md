# ADR-0006: Vertical Slice + Clean Architecture + API-first

**Status**: Accepted · **Date**: 2026-07-29 · **Relates to**: Constitution II.2, III, IV.1, VII.3, X.2

## Context

The constitution's original II.2 described a horizontally-layered backend (Controllers / Services /
Repositories / Entities folders). Three separate questions were left under-answered by that single
sentence, and the six feature specs (001–006) each re-derived answers informally:

- **Where does a use-case's code live?** Layered folders scatter one feature across
  `Controllers/`, `Services/`, `Repositories/`, forcing an edit in three trees to change one behaviour.
- **Which way do dependencies point?** "Layered" says nothing about direction, so business logic can
  drift into taking hard dependencies on EF Core and ASP.NET types.
- **When is the HTTP contract written?** Swagger-from-code (old X.2) means the contract is a
  by-product of the implementation, discovered after the handler exists — the frontend cannot be
  built in parallel, and the contract is whatever the code happened to emit.

These are three orthogonal decisions, and a single "layered architecture" clause conflated them.

## Decision

Adopt three complementary patterns, one per question:

1. **Vertical Slice Architecture (where code lives).** Each feature/use-case is a self-contained slice
   under `Features/<Area>/<UseCase>/`, holding its request, handler, validator, and response shape
   together. Controllers become thin endpoint mappings — one HTTP verb routed to a single
   `MediatR.Send()` call — and hold no business logic. **MediatR** carries the command/query to its
   handler; it is added to the locked backend stack (Constitution III).

2. **Clean Architecture (which way dependencies point).** Dependencies point inward: Domain depends on
   nothing; a slice's handler (Application) depends only on Domain and on abstractions it defines;
   Infrastructure implements those abstractions. This keeps business logic free of framework types.

3. **API-first (when the contract is written).** The OpenAPI contract is authored and reviewed as part
   of a feature's spec/plan, **before** the handler is implemented, and versioned under
   `/docs/contracts/`. Code is validated against the contract, not the reverse (Constitution X.2).
   Frontend HTTP service classes MAY be generated from that contract via `openapi-generator`
   (Constitution VII.3). Swagger UI (VI.5) is retained for local exploration only — the authoring
   direction is what changed, not the runtime tooling.

**Persistence (IV.1 amendment).** Because a slice already owns its data access, a handler MAY call the
EF Core `DbContext` directly as its default persistence path. A separate Repository is optional, not
required. The raw-SQL prohibition in IV.1 is unchanged.

## Alternatives considered

- **Keep the horizontal layered architecture (Controllers/Services/Repositories).** Familiar and
  matches the brief's implicit shape, but scatters each feature across three folders, encourages
  anemic pass-through services, and answers none of the three questions above cleanly. Rejected.
- **Repository + Unit-of-Work over EF Core.** EF Core's `DbContext` is already a Unit of Work and its
  `DbSet<T>` already a repository; a hand-written layer on top mostly duplicates that and complicates
  testing without adding isolation we need at six entities. Left optional rather than mandated.
- **Contract-from-code (Swashbuckle-generated OpenAPI) as the source of truth.** Zero authoring
  overhead, but the contract lags the code and cannot drive parallel frontend work or review before
  implementation. Swagger UI is kept for exploration; it is no longer the contract's source. Rejected
  as the authoring direction.
- **Full onion/hexagonal with ports-and-adapters for every dependency.** More ceremony than a
  six-entity CRUD-plus-reporting app warrants; Clean Architecture's inward-pointing rule captures the
  benefit (framework-independent domain) without the adapter proliferation. Rejected as over-engineered.

## Consequences

- One feature = one folder: changing a use-case touches a single slice, and slices can be built and
  reviewed independently.
- The domain stays framework-free and unit-testable without a database; Infrastructure is swappable.
- The contract exists before the code, so frontend and backend proceed in parallel and the generated
  Angular services stay in lock-step with the API. The cost is authoring `/docs/contracts/` up front
  and keeping code validated against it.
- MediatR adds one dependency and a small amount of request/handler boilerplate per slice — accepted
  in exchange for thin controllers and uniform cross-cutting behaviour (validation, logging) via
  pipeline behaviours.
- **Outstanding**: specs 001–006 were written against the prior layered II.2 and describe
  Controllers/Services/Repositories. They need a revision pass against this ADR **before**
  `/speckit.plan` runs. No code is invalidated (none is written yet); the specs are designs.
  As of Constitution v1.3.0 this is enforced by Governance §5 (a blocking compliance gate),
  not merely recommended here.

## Backward compatibility

None required at the code level: no backend code exists yet, so no compliant implementation is
invalidated — which is why the amendment introducing this ADR is MINOR (Constitution v1.2.0) rather
than MAJOR under Governance §3. The one carried-forward obligation is documentary, not runtime: the
six existing feature specs (001–006) were authored under the old layered wording and remain valid
designs, but each MUST be reconciled with the vertical-slice / Clean Architecture / API-first wording
before it is planned. That obligation is tracked as Constitution Governance §5 and in the constitution's
version-history OUTSTANDING note; this ADR does not rewrite those specs.

## Addendum (2026-07-29): the shared-kernel boundary

The Clean Architecture rule above ("a slice's handler depends only on Domain and on abstractions it
defines") left one case under-specified: **what a feature may depend on from *another* feature.** During
the 001–006 revision this surfaced concretely — 005 Dashboard and 006 Reports reuse 002's/003's scope
policies (`IProjectAccessPolicy` / `ITaskAccessPolicy`), and 003/004 read 002's `Project` and each
other's entities. Left unstated, that reuse could be read as one feature depending on another feature's
Application layer — the exact coupling vertical slices exist to prevent.

**Decision (refinement of this ADR):** cross-cutting **services** and **scope-authorization policies**
are **shared-kernel** abstractions. Their *interfaces* live in the shared kernel — declared in
`docs/shared-contracts.md` (§2 `ICurrentUserService`, §3 the access policies, §6 `IActivityLogService`)
— even though the *implementation* of a policy's scope rules stays with the feature that owns the
resource (002 implements `IProjectAccessPolicy`, etc.). A feature folder may therefore depend on:

1. a **shared-kernel contract** declared in `docs/shared-contracts.md`, or
2. a **shared Domain entity** (the five constitution entities, created in the initial migration),

and on **nothing else outside its own slice** — in particular, **never** on another feature's
Application layer (its `*Command`/`*Query`/`*Handler` types). Cross-feature reuse always routes through
(1) or (2). This turns the per-case judgement made during the 001–006 revision into a standing rule any
future feature (007+) is checked against.

**Alternatives considered.** (a) *Duplicate each scope policy per consumer* — rejected: it would fork
the "who can see this project" rule across 002/005/006 and let them drift. (b) *Let a feature call
another feature's handler directly* (e.g. Dashboard invokes a Projects query handler) — rejected: it is
the coupling this ADR exists to remove and defeats independent slice evolution. (c) *A separate
"SharedKernel" project distinct from shared-contracts* — deferred as a build-time packaging choice;
`docs/shared-contracts.md` is the design-time source of truth either way.

**Consequences.** The boundary is now checkable by a mechanical rule: grep a spec for another feature's
`*Handler`/`*Command`/`*Query` name — any hit is a violation; a reference to a `docs/shared-contracts.md`
abstraction or a shared Domain entity is conformant. The standing rule is stated in
`docs/shared-contracts.md`'s header; this addendum records it under `docs/adr/` per Constitution X.3.
