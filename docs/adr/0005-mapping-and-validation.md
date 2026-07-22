# ADR-0005: Manual mapping extensions + FluentValidation

**Status**: Accepted · **Date**: 2026-07-22 · **Relates to**: Constitution III, V.5, VIII.1

## Context

Two gaps the feature specs assumed but never chose. (1) Entity → DTO mapping: Constitution III locks
libraries and requires an ADR for anything not listed, and no mapper is listed. (2) Validation:
Constitution V.5 mandates validation at the API boundary "using data annotations (FluentValidation is
acceptable but not required)" — but the domain has genuine cross-field rules
(`endDate >= startDate`, task dates within their project's window) that data annotations express
awkwardly at best.

## Decision

**Mapping — manual, via static extension methods.** `ProjectMappingExtensions.ToDetailDto(this Project p)`
and similar, colocated with each feature. No mapping library.

**Validation — FluentValidation**, registered per request-DTO and executed at the API boundary.
Data annotations remain acceptable for trivial single-field constraints, but any rule spanning two
fields, or requiring a lookup, is a FluentValidation rule. Validation failures surface as
`ErrorKind.Validation` → **400** with per-field `errors` in the ProblemDetails body (ADR-0003).

The frontend continues to validate for UX only; server-side validation stays authoritative (V.5).

## Alternatives rejected

- **AutoMapper** — reflection-based, moves mapping errors from compile time to run time, and its
  licensing change makes it a poor default for a new project. Mapperly (source-generated) was a close
  second and remains the fallback if manual mapping becomes tedious at scale.
- **Data annotations only** — cannot express the cross-field date rules without custom
  `ValidationAttribute` classes that are harder to test than a FluentValidation rule.

## Consequences

Mapping is explicit, debuggable, compile-time-checked, and free of a dependency — at the cost of some
boilerplate per DTO, which is acceptable at five entities. FluentValidation adds one dependency and
one validator class per write DTO, and gives uniformly structured field errors across all six modules.
