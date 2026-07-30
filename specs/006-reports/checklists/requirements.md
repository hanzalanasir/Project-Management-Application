# Specification Quality Checklist: Reports

**Purpose**: Validate specification completeness and quality before proceeding to planning
**Created**: 2026-07-22
**Feature**: [spec.md](../spec.md)

> **Convention note**: This project keeps requirements **and** solution design in one merged file
> (the convention established by [001 Auth & RBAC](../../001-auth-rbac/spec.md)). The standard
> "no implementation details" content-quality items are therefore **intentionally not applicable** to
> the Technical Design / Implementation Blueprint sections — they are reframed below to check that the
> *requirements layer* stays user-focused while the *design layer* is deliberately concrete.

## Content Quality

- [x] Requirements layer (Purpose, Actors, User Stories A/B/C, Functional Requirements) is expressed in user/business terms
- [x] Solution layer (Technical Design, Implementation Blueprint) is concrete and build-ready (intended by convention)
- [x] Focused on user value — parameterized, exportable, role-scoped report artifacts over historical windows
- [x] All mandatory sections completed (header, purpose incl. brief-coverage table, actors, scope, access logic, role model, clarifications placeholder, stories, FRs, design, blueprint)

## Requirement Completeness

- [x] No [NEEDS CLARIFICATION] markers remain (genuinely open decisions recorded as OQ-006-01..06; endpoint shape + export architecture decided in T.4)
- [x] Requirements are testable and unambiguous (each FR maps to Given-When-Then + Definition of Done)
- [x] Success criteria are measurable (DoD B.8 — three-role matrix, TeamMember self-only row, named-out-of-scope 403, Dashboard value parity, 422 threshold, exactly-one-audit)
- [x] Acceptance scenarios defined for every story (A–F, Given-When-Then)
- [x] Edge cases identified per story (empty scope, named-out-of-scope, divide-by-zero completion, bucket boundaries, large-window 422, deleted-entity activity)
- [x] Scope is clearly bounded (In scope / Out of scope; custom-report DSL, scheduling/email, persisted artifacts, SignalR all excluded)
- [x] Dependencies and assumptions identified (depends on 001–005; the 003 completion-timestamp flagged as an Assumption, not a retroactive edit)

## Feature Readiness

- [x] Every functional requirement has clear acceptance criteria (FR-001..016 ↔ stories + DoD)
- [x] User scenarios cover the primary flows (catalog, the four reports, export)
- [x] Feature meets the measurable outcomes in the Definition of Done
- [x] Constitution alignment verified (I no-new-persistence + brief-coverage, II incl. SignalR-not-precluded, III Chart.js/jsPDF/CSV, IV.1/IV.4 audit exception, V, VI incl. VI.6 routes + VI.4 pagination, VII.8 export service, VIII, IX, X)
- [x] Reuses shared contracts + existing scope predicates without redefining them (`Result<T>`, `CurrentUser`, `ApplyScope` from 002/003, `IActivityLogService` from 001)
- [x] Read-only posture stated where contracts were evaluated and NOT applied (`CanMutateAsync` — no domain writes; `xmin` — no mutations) **and** the one deliberate write exception (`ReportGenerated`) is explicit
- [x] No new domain entity/table/migration — stated explicitly (Consolidated Read Model, B.1)
- [x] The two consequential technical decisions (endpoint shape; export architecture) are **resolved in Technical Design**, not deferred

## Notes

- **Two decisions resolved in-spec (T.4):** resource-per-report-type endpoints; client-side export (jsPDF for PDF, papaparse for CSV, JSON-only API).
- **Three decisions RESOLVED** (Clarifications, Session 2026-07-22): **OQ-006-05** timezone → **UTC** (fixed, reproducible); **OQ-006-02** large-Activity threshold → **~10,000 rows, forced narrowing (422)**, export fully client-side; the **task-completion-timestamp** Assumption (not a numbered OQ) → 003 gains a **`closed_at`** column (a lightweight, agreed plan-time follow-up to 003; not made by this spec).
- **Cross-spec follow-up recorded:** 003 adds a nullable `closed_at` (set on →Done, cleared on re-open) — see Dependencies. 006 does not edit 003; the change is actioned when 003 is planned/built.
- **Remaining OQ items RESOLVED** (subsequent edit, Clarifications 2026-07-22): **OQ-006-04** report field tables finalized (with the projected-completion formula + uniform closed/re-open counting rule); **OQ-006-06** four reports for v1; **OQ-006-01** scheduled/emailed out of scope; **OQ-006-03** transient artifacts. **All six OQ-006 items are now Resolved** — no open clarifications remain.
- **Audit exception is intentional and documented (B.7):** unlike 005 (writes nothing), Reports writes exactly one `ReportGenerated` audit entry per generation — the sole write, touching no domain entity.
- **Brief-coverage confirmation** in Purpose/Scope shows all six modules now map 001–006 with no uncovered brief requirement.
- Recommended next command: **`/speckit.plan`**.
