namespace ProjectManagementApp.Application.Common.Interfaces;

// US-001-06 — a startup routine, invoked once at boot, NOT a request slice.
/// <summary>Ensures the three roles and one seed user per role exist. Idempotent — safe to call on every startup.</summary>
public interface IDataSeeder
{
    Task SeedAsync(CancellationToken ct);
}
