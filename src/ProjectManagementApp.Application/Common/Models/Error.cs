namespace ProjectManagementApp.Application.Common.Models;

public sealed record Error(ErrorKind Kind, string Message, IReadOnlyDictionary<string, string[]>? Fields = null);
