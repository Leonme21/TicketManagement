using FluentValidation;
using MediatR;
using TicketManagement.Application.Common.Exceptions;

namespace TicketManagement.Application.Common.Behaviors;

/// <summary>
/// 🔥 SENIOR LEVEL: A MediatR pipeline behavior that centrally handles validation.
/// It intercepts every request, finds the corresponding validator, and if validation
/// fails, it throws a single ValidationException that is caught by the global error handler.
/// This enforces validation for all commands and queries without cluttering the handlers.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
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
        // If there are no validators, just continue
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        // Run all validators in parallel and collect the results
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Aggregate all failures
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // If there are any validation failures, throw a single exception
        if (failures.Count != 0)
        {
            throw new TicketManagement.Application.Common.Exceptions.ValidationException(failures);
        }

        // If validation is successful, proceed to the next handler in the pipeline
        return await next();
    }
}