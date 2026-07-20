using CommerceCore.Shared.Exceptions;
using FluentValidation;
using MediatR;

namespace CommerceCore.Application.Common.Behaviors;

/// <summary>
/// Runs every registered FluentValidation validator for TRequest before the actual
/// handler executes. On failure, throws ValidationAppException (translated by the
/// Api's global exception middleware into a 400 ProblemDetails response) rather than
/// letting an invalid request reach business logic at all.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());

        if (errors.Any())
            throw new ValidationAppException(errors);

        return await next();
    }
}
