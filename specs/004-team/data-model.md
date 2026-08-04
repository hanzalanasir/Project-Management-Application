# Phase 1 Data Model: 004 Team Management

**Plan**: [plan.md](plan.md) · **Spec**: [spec.md](spec.md) · **Research**: [research.md](research.md)
**Source of truth**: spec 004 §Consolidated Data Model + B.1.

EF Core 10 + Npgsql · PostgreSQL 18 · snake_case (VIII.2) · `timestamptz` UTC.

---

## 1. What already exists vs. what 004 adds

| Artifact | Created by | 004's action |
|---|---|---|
| `team_members` table, columns, FKs | 001 `InitialCreate` | — already present |
| `project_id` → `projects` **CASCADE** | 001 config | prove by test |
| `user_id` → `users` **CASCADE** | 001 config | prove by test |
| `added_by` → `users` **SET NULL** | 001 config | prove by test |
| **`UNIQUE (project_id, user_id)`** | — | ✅ **`AddTeamMemberIndexes`** — this feature's correctness guarantee |
| Indexes `(project_id)`, `(user_id)` | — | ✅ **`AddTeamMemberIndexes`** |
| Business rules, API, UI | — | ✅ all of 004 |

Spec 004 B.1 was corrected on 2026-07-31 from `AddTeamMembersTable` to **`AddTeamMemberIndexes`**, matching
002 and 003 — the table comes from 001's `InitialCreate`.

**Why the unique constraint is added here rather than in 001:** it is not a performance index but *this
feature's* correctness mechanism (research R-2), and **only this feature inserts into the table**. 002 and
003 read it; neither can create a duplicate.

---

## 2. `TeamMember` entity (`ProjectManagementApp.Domain/Entities/TeamMember.cs`)

| Property | Type | Null | Notes |
|---|---|---|---|
| `Id` | `Guid` | ✘ | PK — surrogate, not composite (OQ-004-06) |
| `ProjectId` | `Guid` | ✘ | FK → `projects`, **CASCADE**. From the **route**, never the body |
| `UserId` | `Guid` | ✘ | FK → `users`, **CASCADE** |
| `AddedBy` | `Guid?` | ✔ | FK → `users`, **SET NULL** — `null` = system/seed |
| `CreatedAt` | `DateTimeOffset` | ✘ | UTC |
| `Project` | `Project` | — | navigation |
| `User` | `ApplicationUser` | — | navigation |

### The three absences, each deliberate

| Absent | Why |
|---|---|
| **No role/permission column** | **The single most important schema fact of this feature.** A membership records *that* a user is on a team, full stop. Authority comes from the user's global role (001). This is what keeps RBAC single-sourced and is why "per-project roles" are out of scope. |
| **No `updated_at`, no `xmin`** | The row has no mutable field — it is added or removed, never edited. Nothing for a row version to protect (research R-2, shared-contracts §5). |
| **No `PagedResult<T>` on read** | The roster is a bounded, human-scale collection; VI.4's ">50 items" trigger does not fire (research R-4). |

**Surrogate `Id` rather than a composite PK** (OQ-004-06): it keeps `activity_logs.entity_id` a clean
single uuid, consistent with every other entity's audit rows.

---

## 3. Authorization — binary, not graduated

`TeamAccessPolicy` in `.Application/Common/Authorization/`, implementing the shared-kernel
`ITeamAccessPolicy` (shared-contracts §3).

| Operation | Admin | ProjectManager | TeamMember |
|---|---|---|---|
| **View roster** (`CanViewTeamAsync`) | any project | **owner *or* member** | member only |
| **Add / remove** (`CanManageTeamAsync`) | any project | **owner only** | deny (also blocked at the role gate) |

Both decisions are evaluated against the **parent project**, loaded by the route's `projectId`. There is no
`ApplyScope` — every operation is already pinned to one project, so there is no cross-collection query to
scope (research R-1).

**The ProjectManager row is the interesting one.** Since any active user may be added to any team, a
manager can be a *member* of a project they do not own — they may then **view** that roster but **not**
manage it. That divergence is why `CanViewTeamAsync` and `CanManageTeamAsync` cannot be one method.

**No graduated mutation model.** 003 needed `TaskMutation` because a task row has fields a user may
partially change. A membership row has no editable field, so there is no "partial write" to permit — you
can add it or remove it. Two decisions, not a matrix.

---

## 4. Migration `AddTeamMemberIndexes`

```
CREATE UNIQUE INDEX ux_team_members_project_id_user_id ON team_members (project_id, user_id);
CREATE INDEX        ix_team_members_project_id         ON team_members (project_id);
CREATE INDEX        ix_team_members_user_id            ON team_members (user_id);
```

| Index | Serves |
|---|---|
| **UNIQUE `(project_id, user_id)`** | One membership per user per project — **and the concurrency guarantee** (R-2/R-3). Also serves 002's TeamMember project-scope join and 003's assignee-pool validation |
| `(project_id)` | Roster read; cascade cleanup on project delete |
| `(user_id)` | 002's "projects I'm a member of" scope predicate; cascade on user delete |

The unique index is the one that carries semantics — the other two are performance only.

---

## 5. Write flows

### Add (`POST /projects/{projectId}/team`)
1. Role gate: `Admin,ProjectManager` — else **403**
2. Load parent project → not found **404**; `CanManageTeamAsync` → **403**
3. Target user exists → else **404**; **is active** → else **400** *(the only eligibility gate — any active user of any global role is eligible)*
4. Not already a member → else **409** *(friendly pre-check)*
5. Insert + `TeamMemberAdded` audit in **one** `SaveChangesAsync`
6. **Catch SQLSTATE 23505** on the unique index → **409** *(the actual race guarantee — R-3)*
7. **201** + `Location: /api/projects/{projectId}/team/{userId}`

### Remove (`DELETE /projects/{projectId}/team/{userId}`)
1. Role gate → **403**
2. Load parent project → **404**; `CanManageTeamAsync` → **403**
3. Membership exists → else **404**
4. **Open-task check** — any `tasks` row with `project_id` = this project, `assignee_id` = this user, and
   `status != Done` → **409** with a dependency message listing the blocking tasks. **Nothing is written,
   including no audit row** (R-5, spec B.7)
5. `TeamMemberRemoved` audit written **before** the delete, same transaction
6. **204**

No `If-Match` on either endpoint (R-2).

---

## 6. Transactional invariants

1. **Add/remove and audit commit together** in one `SaveChangesAsync` (IV.4).
2. **Remove audits before deletion**, so the record survives.
3. **Uniqueness is guaranteed by the database**, not by the application pre-check — the check is an
   optimization for a better message (R-3).
4. **A blocked removal is a no-op**: no membership change, no audit row, no task mutation.
5. `activity_logs` rows survive cascade deletion of the membership, its project, or its user.

---

## 7. Deliberate non-goals

- **No per-project roles or permission overrides** — the feature exists partly to *avoid* a second
  permission system.
- **No bulk import** of members.
- **No `xmin`**, **no `PagedResult<T>`**, **no `ApplyScope`**, **no `TaskMutation`-style matrix** — each
  absent for a reason recorded in research §B.
- **No mutation of `tasks`** — 004 reads that table to block a removal and never writes it (R-5).
- **No new shared-kernel abstraction.** 004 *implements* `ITeamAccessPolicy` (§3) and *consumes*
  `IApplicationDbContext` (§7) and `IActivityLogService` (§6).
