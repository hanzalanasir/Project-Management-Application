namespace ProjectManagementApp.Application.Common.Models;

public enum ErrorKind
{
    Validation,
    Unauthenticated,
    Forbidden,
    NotFound,
    Conflict,
    UnprocessableContent,
    Unexpected
}
