using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Projects.DeleteProject;

// No If-Match — a delete has no lost-update failure mode (ADR-0007 §3).
public sealed record DeleteProjectCommand(Guid Id) : IRequest<Result>;
