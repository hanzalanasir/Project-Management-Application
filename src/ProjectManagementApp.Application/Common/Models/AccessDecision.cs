namespace ProjectManagementApp.Application.Common.Models;

public sealed record AccessDecision(bool Allowed, string? Reason = null);
