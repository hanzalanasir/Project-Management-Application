# Entity-Relationship Diagram

Generated from `src/ProjectManagementApp.Infrastructure/Persistence/Migrations/*_InitialCreate.cs`
(Constitution X.4). All five constitution entities are created here (research.md R-10) —
`projects`, `tasks`, and `team_members` are table-only in 001; their behavior is owned by
002/003/004 respectively.

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
