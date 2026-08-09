using System.Text.Json;
using FluentValidation;
using MediatR;
using ProjectManagementApp.Application.Common.Models;

namespace ProjectManagementApp.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = results.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

            if (failures.Count > 0)
            {
                // FluentValidation.PropertyName is the C# property name (PascalCase). The rest of
                // the API is camelCase (System.Text.Json's default), so the contract's errors
                // example (docs/contracts/auth.v1.yaml) is camelCase too — match it here.
                var fields = failures
                    .GroupBy(f => JsonNamingPolicy.CamelCase.ConvertName(f.PropertyName))
                    .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());

                var error = new Error(ErrorKind.Validation, "Validation failed", fields);
                return CreateFailureResponse(error);
            }
        }

        return await next(cancellationToken);
    }

    private static TResponse CreateFailureResponse(Error error)
    {
        // TResponse is always Result or Result<T> in this codebase (ADR-0003).
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failureMethod = responseType.GetMethod(nameof(Result<object>.Failure), new[] { typeof(Error) })!;
            return (TResponse)failureMethod.Invoke(null, new object[] { error })!;
        }

        throw new InvalidOperationException(
            $"{nameof(ValidationBehavior<TRequest, TResponse>)} only supports handlers returning {nameof(Result)} or {nameof(Result<object>)}<T>.");
    }
}
