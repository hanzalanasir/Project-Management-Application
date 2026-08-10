// Shared by every "type a user/entity ID" field (Add Team Member, Create Task's assignee) — the
// API's ids are Guids, not sequential numbers, and this catches an obviously-wrong value (e.g.
// "1") with a specific field error instead of letting it reach the server as an opaque
// JSON-binding failure.
export const GUID_PATTERN = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;
