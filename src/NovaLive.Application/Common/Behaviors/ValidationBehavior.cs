using FluentValidation;
using MediatR;
using NovaLive.Domain.Common;

namespace NovaLive.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .Select(failure => new Error(failure.PropertyName, failure.ErrorMessage))
            .GroupBy(error => error.Code)
            .Select(group => new Error(
                ErrorType.Validation,
                group.Key,
                string.Join("; ", group.Select(item => item.Message).Distinct())))
            .ToArray();

        if (errors.Length > 0)
        {
            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)ValidationResult.WithErrors(errors);
            }

            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var resultType = typeof(TResponse).GenericTypeArguments[0];
                var validationResult = typeof(ValidationResult<>)
                    .MakeGenericType(resultType)
                    .GetMethod(nameof(ValidationResult.WithErrors))!
                    .Invoke(null, [errors])!;

                return (TResponse)validationResult;
            }

            throw new ValidationException(validationResults.SelectMany(r => r.Errors).Where(f => f != null));
        }

        return await next(cancellationToken);
    }
}
