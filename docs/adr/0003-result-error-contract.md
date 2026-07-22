# ADR-0003: `Result<T>` + a shared mapper for the service → controller error contract

**Status**: Accepted · **Date**: 2026-07-22 · **Relates to**: Constitution II.2, VI.2, VI.3

## Context

Constitution II.2 puts all business logic in Services and forbids controllers from doing more than
validation and delegation. Services must therefore communicate *why* an operation failed — not found,
forbidden by scope, invalid input — without the controller re-deriving it. Feature specs 001 and 002
already use `Result<T>` in every service signature; the type needed a definition.

## Decision

Services return an explicit **`Result` / `Result<T>`** carrying a success value or an `Error` with an
`ErrorKind`. A single shared mapper translates `ErrorKind` to an HTTP status and an **RFC 7807
ProblemDetails** body (Constitution VI.3), so the mapping exists in exactly one place.

`ErrorKind` → status: `Validation` → 400 · `Unauthenticated` → 401 · `Forbidden` → 403 ·
`NotFound` → 404 · `Conflict` → 409 · `Unexpected` → 500.

Exceptions remain for *exceptional* conditions only (bugs, infrastructure failure), caught by
exception-handling middleware that emits a 500 ProblemDetails. Expected outcomes never throw.

The concrete shapes are defined in [docs/shared-contracts.md](../shared-contracts.md).

## Alternatives rejected

**Domain exceptions + middleware** (`NotFoundException`, `ForbiddenException`) gives cleaner
signatures but uses exceptions for ordinary control flow, is slower on hot paths, hides outcomes from
the type system, and would require rewriting both existing specs' interfaces.

## Consequences

Service signatures are explicit and unit-testable without asserting on thrown exceptions. Controllers
become uniformly thin — call the service, hand the result to the mapper. Every new feature inherits
the same status-code behaviour for free, which is what keeps VI.2 consistent across six modules.
A small amount of ceremony per service method is accepted in exchange.
