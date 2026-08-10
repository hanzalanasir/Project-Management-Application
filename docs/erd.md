# Entity-Relationship Diagram

Generated from `src/ProjectManagementApp.Infrastructure/Persistence/Migrations/*_InitialCreate.cs`
(Constitution X.4). All five constitution entities are created here (research.md R-10) —
`projects`, `tasks`, and `team_members` are table-only in 001; their behavior is owned by
002/003/004 respectively. `projects`' indexes were added afterward by 002's `AddProjectIndexes`
migration (data-model.md §4), and `tasks`' by 003's `AddTaskIndexes` migration — see Notes below;
the table shapes themselves are unchanged by either migration (003 adds no table, per tasks.md's
own blocking-prerequisites note).

```mermaid
erDiagram
    users ||--o{ refresh_tokens : "has"
    users ||--o{ projects : "owns"
    users ||--o{ tasks : "is assigned"
    users ||--o{ team_members : "is a member of"
    users ||--o{ team_members : "added (nullable)"
    users }o--o{ roles : "user_roles"
    projects ||--o{ tasks : "contains"
    projects ||--o{ team_members : "has roster"

    users {
        uuid id PK
        string full_name
        string email
        string normalized_email
        string password_hash
        boolean is_active
        timestamptz created_at
        timestamptz updated_at
        xid xmin "row version"
    }

    roles {
        uuid id PK
        string name
        string normalized_name
    }

    user_roles {
        uuid user_id FK
        uuid role_id FK
    }

    refresh_tokens {
        uuid id PK
        uuid user_id FK "CASCADE"
        string token_hash UK "SHA-256, unique"
        timestamptz expires_at
        timestamptz created_at
        timestamptz revoked_at "nullable"
        string replaced_by_token "nullable"
    }

    activity_logs {
        uuid id PK
        uuid actor_id "nullable, no FK constraint"
        string action
        string entity_type
        string entity_id
        timestamptz timestamp
        string change_summary
    }

    projects {
        uuid id PK
        string name
        string description "nullable"
        date start_date
        date end_date "nullable"
        string status
        uuid owner_id FK "RESTRICT"
        timestamptz created_at
        timestamptz updated_at
        xid xmin "row version"
    }

    tasks {
        uuid id PK
        uuid project_id FK "CASCADE"
        string title
        string description "nullable"
        string status
        string priority
        date due_date "nullable"
        uuid assignee_id FK "RESTRICT, nullable"
        timestamptz closed_at "nullable, derived"
        timestamptz created_at
        timestamptz updated_at
        xid xmin "row version"
    }

    team_members {
        uuid id PK
        uuid project_id FK "CASCADE"
        uuid user_id FK "CASCADE"
        uuid added_by FK "SET NULL, nullable"
        timestamptz created_at
    }
```

## Notes

- **`xmin`** (PostgreSQL system column) is mapped as the optimistic-concurrency token on `users`,
  `projects`, and `tasks` — the three tables that are contended-updated (ADR-0004). `refresh_tokens`
  and `activity_logs` are append-only/system tables and deliberately excluded (shared-contracts.md §5).
  `team_members` has no mutable field, so there is nothing for a row version to protect — its
  correctness guarantee is the unique index `(project_id, user_id)` instead (added by 004).
- **`activity_logs.actor_id`** has no foreign-key constraint — audit rows must survive the deletion
  of the actor that created them.
- The four unused ASP.NET Core Identity tables (`user_claims`, `user_logins`, `user_tokens`,
  `role_claims`) are created by Identity's stores and are intentionally empty in this application
  (data-model.md §6).
- **`projects` indexes** (002's `AddProjectIndexes` migration, data-model.md §4 — not shown in the
  diagram above since Mermaid ER syntax has no index notation):
  - `ix_projects_owner_id` — the ProjectManager scope predicate; the single hottest filter (already
    existed from 001's FK convention; this migration keeps it explicit rather than duplicating it).
  - `ix_projects_status` — the `?status=` list filter.
  - `ix_projects_owner_id_status` — the common composite: a manager filtering their own projects by status.
  - `ix_projects_name_trgm` — a GIN trigram index (`pg_trgm` extension) on `name`, serving the
    case-insensitive interior-substring `?search=` filter; a B-tree index cannot serve `%term%`.
  - Deliberately **no** index for the TeamMember scope on `projects` — that predicate resolves
    through `team_members`, whose `(project_id, user_id)` unique index (004) is what makes it fast.
- **`tasks` indexes** (003's `AddTaskIndexes` migration, data-model.md §4 — likewise not shown in
  the diagram above). `ix_tasks_project_id` and `ix_tasks_assignee_id` already existed from 001's
  default FK-index convention, so this migration adds only the four not already covered:
  - `ix_tasks_status` — the `?status=` list filter.
  - `ix_tasks_project_id_status` — the nested-route composite: a task list filtered by status within
    one project.
  - `ix_tasks_assignee_id_status` — the TeamMember scope predicate composed with a status filter.
  - `ix_tasks_title_trgm` — a GIN trigram index (`pg_trgm` extension, already enabled by 002) on
    `title`, serving the case-insensitive interior-substring `?search=` filter, same reasoning as
    `ix_projects_name_trgm`.
