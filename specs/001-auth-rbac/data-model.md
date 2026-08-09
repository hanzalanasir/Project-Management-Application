# Phase 1 Data Model: 001 Auth & RBAC

**Plan**: [plan.md](plan.md) · **Spec**: [spec.md](spec.md) · **Research**: [research.md](research.md)
**Source of truth**: spec 001 §Consolidated Data Model + B.1. This file adds only the **CLR-side shape**
and the **migration plan** — it introduces no column the spec did not already commit to.

EF Core 10 Code-First + Npgsql · PostgreSQL 18 · snake_case identifiers (VIII.2) · all timestamps
`timestamptz` in UTC · migration name `InitialCreate` (IV.2: descriptive, not a bare timestamp).

---

## 1. Scope of the `InitialCreate` migration — read this first

Per [research.md R-10](research.md), `InitialCreate` creates **all five constitution entities**, not just
the ones 001 owns. This is a cross-spec obligation recorded in 002, 003, and 004's Assumptions: *a feature
owns an entity's API/UI/rules, not the existence of its table.*

| Table | Created by `InitialCreate` | Behavior owned by | 001 does |
|---|---|---|---|
| `users`, `roles`, `user_roles` | ✅ | **001** | full CRUD via Identity |
| `refresh_tokens` | ✅ | **001** | full lifecycle |
| `activity_logs` | ✅ | **001** (written by all) | writes User-targeted rows |
| `projects` | ✅ | 002 | nothing — table only |
| `tasks` | ✅ | 003 | nothing — table only |
| `team_members` | ✅ | 004 | nothing — table only |

Omitting the last three would leave 002's TeamMember scope predicate and 003's assignee-validation join
uncompilable until 004 ships. Their **entity classes and FK/cascade rules are authored now**; their
handlers, endpoints, and UI are not.

---

## 2. Domain entities (`ProjectManagementApp.Domain/Entities/`)

### `ApplicationUser : IdentityUser<Guid>`
Identity supplies `Id`, `UserName`, `NormalizedUserName`, `Email`, `NormalizedEmail`, `PasswordHash`,
`SecurityStamp`, `ConcurrencyStamp`, `EmailConfirmed`, `LockoutEnd`, `LockoutEnabled`, `AccessFailedCount`.

| Added property | Type | Null | Notes |
|---|---|---|---|
| `FullName` | `string` | ✘ | varchar(200) |
| `IsActive` | `bool` | ✘ | default `true`; `false` blocks login **and** refresh (FR-004) |
| `CreatedAt` / `UpdatedAt` | `DateTimeOffset` | ✘ | UTC |
| `Version` | `uint` | ✘ | mapped to PostgreSQL `xmin`, `IsRowVersion()` (ADR-0004) |
| `RefreshTokens` | `ICollection<RefreshToken>` | — | navigation |
| `OwnedProjects` | `ICollection<Project>` | — | navigation (002 uses it; IV.3 requires it) |
| `AssignedTasks` | `ICollection<TaskItem>` | — | navigation (003) |
| `TeamMemberships` | `ICollection<TeamMember>` | — | navigation (004) |

**Business rule — exactly one role per user.** `user_roles` is physically a many-to-many join (Identity's
shape), but the domain permits exactly one row per user. Enforced by the registration/seed paths and
asserted by test; it is **not** a database constraint, and that is deliberate — Identity's own store APIs
assume the join table. **US-001-08 (change a user's role) is the first feature capability to *change* an
existing `user_roles` row rather than only insert one at registration/seed time**, and it preserves this
same one-role invariant (remove the old row, add the new one, in the same transaction).

**No new column for US-001-07/08/09.** Admin user management operates entirely on fields already listed
above: `IsActive` (already present, default `true` — US-001-09 is simply the first and only place that
*sets* it administratively; nothing else in 001–006 ever flips it) and `Version`/`xmin` (already present —
US-001-08/09's writes are the first in 001 to require `If-Match`. Since 001 is now the first feature built,
it is also the first to need the shared `ETagExtensions` helper (`Api/Common/`), so 001 creates it rather
than 002 — see research R-15). No migration beyond `InitialCreate` is needed.

### `ApplicationRole : IdentityRole<Guid>`
No added properties. Exactly three rows, seeded: `Admin`, `ProjectManager`, `TeamMember`.

### `RefreshToken`
| Property | Type | Null | Notes |
|---|---|---|---|
| `Id` | `Guid` | ✘ | PK |
| `UserId` | `Guid` | ✘ | FK → `users`, **ON DELETE CASCADE** |
| `TokenHash` | `string` | ✘ | varchar(256), **unique**; SHA-256 of the opaque token — the raw value is never persisted |
| `ExpiresAt` | `DateTimeOffset` | ✘ | |
| `CreatedAt` | `DateTimeOffset` | ✘ | |
| `RevokedAt` | `DateTimeOffset?` | ✔ | set by logout and by rotation |
| `ReplacedByToken` | `string?` | ✔ | varchar(256), hash of the successor — the rotation chain |
| `User` | `ApplicationUser` | — | navigation |

Computed (not mapped): `IsActive => RevokedAt is null && ExpiresAt > now`.
**No `xmin`** — the row is inserted and revoked, never contended-updated.

### `ActivityLog`
| Property | Type | Null | Notes |
|---|---|---|---|
| `Id` | `Guid` | ✘ | PK |
| `ActorId` | `Guid?` | ✔ | **null = system/seed**; deliberately a *soft* FK — no constraint, so audit survives user deletion |
| `Action` | `string` | ✘ | varchar(100), e.g. `UserRegistered` |
| `EntityType` | `string` | ✘ | varchar(100), e.g. `User` |
| `EntityId` | `string` | ✘ | varchar(64) — string, not uuid, so a logical target like `Report` (006) fits |
| `Timestamp` | `DateTimeOffset` | ✘ | UTC |
| `ChangeSummary` | `string` | ✘ | varchar(1000); never contains passwords or raw tokens |

Append-only. Never cascaded away by any FK in the model.

### Entities created but not owned by 001
`Project`, `TaskItem` (CLR name avoids colliding with `System.Threading.Tasks.Task`), and `TeamMember` are
authored to the field lists in 002 §Data Model, 003 §Data Model (incl. `closed_at`), and 004 §Data Model
respectively. 001 adds no rules to them.

---

## 3. Enumerations (`ProjectManagementApp.Domain/Enums/`)

| Enum | Values | Persisted as |
|---|---|---|
| `Role` | `Admin, ProjectManager, TeamMember` | Identity `roles` rows (not a column) |
| `AuditAction` | **All 20 values across 001–006** — see note below | `activity_logs.action` string |
| **`TaskMutation`** | `Create, FullEdit, StatusChange, Reassign, Delete` | **never persisted** — authorization vocabulary |
| `ProjectStatus`, `TaskStatus`, `TaskPriority` | per 002 B.2 / 003 B.2 | string columns |

> **`TokenType` removed 2026-08-05** (`/speckit.analyze` finding C1): this table previously listed a
> `TokenType` (`Access, Refresh`) enum. It was created by no task and referenced by no interface signature
> anywhere in spec 001 or this file — vestigial from an earlier draft. Simpler to remove than to carry a
> type nothing consumes.

> **`AuditAction` is one shared enum carrying every feature's values**, created here in full — exactly like
> `ProjectStatus`/`TaskStatus`/`TaskPriority`, which 001 also creates on behalf of 002/003. Later features
> **consume** values; none edits this file:
>
> | Feature | Values |
> |---|---|
> | 001 | `UserRegistered, UserLoggedIn, UserLoggedOut, TokenRefreshed, UserDeactivated, UserSeeded, UserRoleChanged, UserReactivated` |
> | 002 | `ProjectCreated, ProjectUpdated, ProjectDeleted, ProjectOwnerChanged` |
> | 003 | `TaskCreated, TaskUpdated, TaskStatusChanged, TaskReassigned, TaskDeleted` |
> | 004 | `TeamMemberAdded, TeamMemberRemoved` |
> | 005 | *(none — writes nothing)* |
> | 006 | `ReportGenerated` |
>
> Creating it with only 001's six original values would break 002's very first audited write. Found during
> 006's planning pre-flight; it is the same shared-member-consumed-later pattern as `TaskMutation` and
> `QueryScopedAsync`, which the §2/§3/§6/§7 sweep missed because `AuditAction` is a Domain enum rather
> than a shared-contracts type. **`UserRoleChanged`/`UserReactivated` added 2026-08-05** alongside
> US-001-07/08/09 (`/speckit.analyze` finding F1's remediation) — bringing 001's own count from six to
> eight, and the shared total from 18 to 20.

> **`TaskMutation` lives in `.Application/Common/Models/`, not `Domain/Enums/`**, beside `AccessDecision` —
> its sibling in the very same method signature. It is authorization vocabulary, never domain state, and it
> **must be created in 001** even though only 003 consumes its values: 001's T022 authors the shared-kernel
> `ITaskAccessPolicy`, whose `CanMutateAsync(TaskItem, TaskMutation, …)` signature references it. Deferring
> it to 003 would leave 001's own Foundational phase non-compiling (003 research R-1).

All persisted enums are stored **as strings**, not ints — readability in the database and immunity to
reordering during migrations (002 B.2).

---

## 4. EF configuration (`Infrastructure/Persistence/Configurations/`)

One `IEntityTypeConfiguration<T>` per entity; **no fluent config in `OnModelCreating`** beyond
`ApplyConfigurationsFromAssembly`.

- **snake_case naming** applied globally by a naming convention, so `FullName` → `full_name` without
  per-property mapping.
- **`xmin`**: `builder.Property(u => u.Version).HasColumnName("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsRowVersion();`
- **Identity table renames**: `AspNetUsers` → `users`, `AspNetRoles` → `roles`, `AspNetUserRoles` →
  `user_roles`. The four unused Identity tables (`AspNetUserClaims`, `AspNetUserLogins`,
  `AspNetUserTokens`, `AspNetRoleClaims`) are still created — Identity's stores expect them — and are
  simply unused; see §6.

### Indexes (001-owned tables)
| Table | Index |
|---|---|
| `users` | unique(`normalized_email`), unique(`normalized_user_name`) — Identity defaults |
| `roles` | unique(`normalized_name`) |
| `refresh_tokens` | unique(`token_hash`); (`user_id`) |
| `activity_logs` | (`entity_type`, `entity_id`); (`actor_id`); (`timestamp`) |

### Delete behavior (explicit and intentional — IV.3)
| Relationship | Behavior | Why |
|---|---|---|
| `refresh_tokens.user_id` → `users` | **CASCADE** | tokens are meaningless without their user |
| `activity_logs.actor_id` | *(no FK)* | audit must outlive the actor |
| `projects.owner_id` → `users` | **RESTRICT** | reassign ownership before deleting a user (002) |
| `tasks.assignee_id` → `users` | **RESTRICT** | never silently orphan assigned work (003) |
| `tasks.project_id` → `projects` | **CASCADE** | a task has no meaning without its project (003) |
| `team_members.project_id` → `projects` | **CASCADE** | (004) |
| `team_members.user_id` → `users` | **CASCADE** | (004) |
| `team_members.added_by` → `users` | **SET NULL** | the membership fact outlives whoever created it (004) |

---

## 5. Transactional invariants

1. **Every domain write and its `activity_logs` row commit in one `SaveChangesAsync`** (IV.4). A user
   change can never exist without its audit entry.
2. **Deletes audit before removal** so the audit row survives (applies from 002 onward; 001 has no delete).
3. **Refresh rotation is atomic**: revoke old (`RevokedAt`, `ReplacedByToken`) + insert new + write
   `TokenRefreshed` audit, in a single transaction. A crash mid-rotation must never leave two live tokens.

---

## 6. Deliberate non-goals in this model

- **No `xmin` on `refresh_tokens` or `activity_logs`** — append-only/system tables are excluded by
  shared-contracts §5.
- **The four unused Identity tables are not dropped.** Removing them means custom stores; the cost is
  four empty tables. Revisit only if claims/external logins are truly never needed.
- **`user_roles` keeps its many-to-many shape** even though the domain allows one role — see §2.
- **No soft-delete column anywhere.** Hard delete with cascade is the resolved decision (002
  Clarifications); `activity_logs` is the recovery story.
