namespace TicketManagement.Domain.Common;

/// <summary>
/// ?? BIG TECH LEVEL: Comprehensive Result Pattern implementation
/// Supports typed errors with HTTP status code mapping for clean API responses
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    
    /// <summary>
    /// Gets the result status based on the error type
    /// </summary>
    public ResultStatus Status => Error.Type switch
    {
        ErrorType.None => ResultStatus.Success,
        ErrorType.Validation => ResultStatus.Invalid,
        ErrorType.NotFound => ResultStatus.NotFound,
        ErrorType.Unauthorized => ResultStatus.Unauthorized,
        ErrorType.Forbidden => ResultStatus.Forbidden,
        ErrorType.Conflict => ResultStatus.Conflict,
        ErrorType.RateLimitExceeded => ResultStatus.RateLimitExceeded,
        ErrorType.Internal => ResultStatus.Error,
        _ => ResultStatus.Error
    };

    protected Result(bool isSuccess, Error error)
    {
        switch (isSuccess)
        {
            case true when error != Error.None:
                throw new InvalidOperationException("A successful result cannot have an error.");
            case false when error == Error.None:
                throw new InvalidOperationException("A failed result must have an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    // === SUCCESS FACTORY METHODS ===
    public static Result Success() => new(true, Error.None);

    public static Result<TValue> Success<TValue>(TValue value) => new(value);

    // === FAILURE FACTORY METHODS ===
    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Failure<TValue>(Error error) => new(error);

    // === STRING-BASED FAILURE (convenience methods) ===
    public static Result Failure(string message) => 
        new(false, Error.Validation("Validation.Error", message));

    public static Result<TValue> Failure<TValue>(string message) => 
        new(Error.Validation("Validation.Error", message));

    // === VALIDATION ERRORS (400 Bad Request) ===
    public static Result Invalid(string message) =>
        new(false, Error.Validation("Validation.Invalid", message));

    public static Result<TValue> Invalid<TValue>(string message) =>
        new(Error.Validation("Validation.Invalid", message));

    // === NOT FOUND ERRORS (404 Not Found) ===
    public static Result NotFound(string message) =>
        new(false, Error.NotFound("Resource.NotFound", message));

    public static Result<TValue> NotFound<TValue>(string message) =>
        new(Error.NotFound("Resource.NotFound", message));

    public static Result NotFound(string entityName, object entityId) =>
        new(false, Error.NotFound($"{entityName}.NotFound", $"{entityName} with ID '{entityId}' was not found."));

    public static Result<TValue> NotFound<TValue>(string entityName, object entityId) =>
        new(Error.NotFound($"{entityName}.NotFound", $"{entityName} with ID '{entityId}' was not found."));

    // === FORBIDDEN ERRORS (403 Forbidden) ===
    public static Result Forbidden(string message) =>
        new(false, Error.Forbidden("Access.Forbidden", message));

    public static Result<TValue> Forbidden<TValue>(string message) =>
        new(Error.Forbidden("Access.Forbidden", message));

    // === UNAUTHORIZED ERRORS (401 Unauthorized) ===
    public static Result Unauthorized(string message) =>
        new(false, Error.Unauthorized("Access.Unauthorized", message));

    public static Result<TValue> Unauthorized<TValue>(string message) =>
        new(Error.Unauthorized("Access.Unauthorized", message));

    // === CONFLICT ERRORS (409 Conflict) ===
    public static Result Conflict(string message) =>
        new(false, Error.Conflict("Resource.Conflict", message));

    public static Result<TValue> Conflict<TValue>(string message) =>
        new(Error.Conflict("Resource.Conflict", message));

    // === RATE LIMIT ERRORS (429 Too Many Requests) ===
    public static Result RateLimitExceeded(string message) =>
        new(false, Error.RateLimitExceeded("RateLimit.Exceeded", message));

    public static Result<TValue> RateLimitExceeded<TValue>(string message) =>
        new(Error.RateLimitExceeded("RateLimit.Exceeded", message));

    // === INTERNAL ERRORS (500 Internal Server Error) ===
    public static Result InternalError(string message) =>
        new(false, Error.Internal("Internal.Error", message));

    public static Result<TValue> InternalError<TValue>(string message) =>
        new(Error.Internal("Internal.Error", message));

    // === UTILITY METHODS ===
    public static Result FirstFailureOrSuccess(params Result[] results)
    {
        foreach (Result result in results)
        {
            if (result.IsFailure)
            {
                return result;
            }
        }

        return Success();
    }

    /// <summary>
    /// Creates a Result from an async operation that may throw
    /// </summary>
    public static async Task<Result> TryAsync(Func<Task> operation, string errorMessage = "An error occurred")
    {
        try
        {
            await operation();
            return Success();
        }
        catch (Exception)
        {
            return InternalError(errorMessage);
        }
    }

    /// <summary>
    /// Creates a Result<T> from an async operation that may throw
    /// </summary>
    public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> operation, string errorMessage = "An error occurred")
    {
        try
        {
            var value = await operation();
            return Success(value);
        }
        catch (Exception)
        {
            return InternalError<T>(errorMessage);
        }
    }
}

/// <summary>
/// ?? BIG TECH LEVEL: Result status enum for HTTP status code mapping
/// </summary>
public enum ResultStatus
{
    Success = 200,
    Invalid = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    Conflict = 409,
    RateLimitExceeded = 429,
    Error = 500
}

/// <summary>
/// ?? BIG TECH LEVEL: Generic Result with value support
/// </summary>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Cannot access value of a failed result. Error: {Error}");

    protected internal Result(Error error)
        : base(false, error)
    {
        _value = default;
    }

    protected internal Result(TValue value)
        : base(true, Error.None)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the value or a default if the result is a failure
    /// </summary>
    public TValue? GetValueOrDefault(TValue? defaultValue = default) =>
        IsSuccess ? _value : defaultValue;

    /// <summary>
    /// Maps the value to a new type if successful
    /// </summary>
    public Result<TNew> Map<TNew>(Func<TValue, TNew> mapper) =>
        IsSuccess ? Result.Success(mapper(_value!)) : Result.Failure<TNew>(Error);

    /// <summary>
    /// Binds to a new result if successful
    /// </summary>
    public Result<TNew> Bind<TNew>(Func<TValue, Result<TNew>> binder) =>
        IsSuccess ? binder(_value!) : Result.Failure<TNew>(Error);

    /// <summary>
    /// Executes an action if the result is successful
    /// </summary>
    public Result<TValue> Tap(Action<TValue> action)
    {
        if (IsSuccess)
            action(_value!);
        return this;
    }

    /// <summary>
    /// Pattern matching support
    /// </summary>
    public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess(_value!) : onFailure(Error);

    public static implicit operator Result<TValue>(TValue value) => Success(value);
    public static implicit operator Result<TValue>(Error error) => Failure(error);

    // ✅ FIXED: Use 'new' keyword intentionally to shadow base class methods with typed versions
    public static Result<TValue> Success(TValue value) => new(value);
    public new static Result<TValue> Failure(Error error) => new(error);
    public new static Result<TValue> Failure(string message) => new(Error.Validation("Validation.Error", message));
    public new static Result<TValue> Invalid(string message) => new(Error.Validation("Validation.Invalid", message));
    public new static Result<TValue> NotFound(string message) => new(Error.NotFound("Resource.NotFound", message));
    public new static Result<TValue> Forbidden(string message) => new(Error.Forbidden("Access.Forbidden", message));
    public new static Result<TValue> Unauthorized(string message) => new(Error.Unauthorized("Access.Unauthorized", message));
    public new static Result<TValue> Conflict(string message) => new(Error.Conflict("Resource.Conflict", message));
    public new static Result<TValue> RateLimitExceeded(string message) => new(Error.RateLimitExceeded("RateLimit.Exceeded", message));
    public new static Result<TValue> InternalError(string message) => new(Error.Internal("Internal.Error", message));
}