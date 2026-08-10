using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Features.Tasks.DeleteTask;

// No If-Match — a delete has no lost-update failure mode (ADR-0007 §3, same as 002's DeleteProject).
public sealed record DeleteTaskCommand(Guid Id) : IRequest<Result>;
