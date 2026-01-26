namespace TicketManagement.Application.Common.Models;

/// <summary>
/// Patrón Result para evitar excepciones en flujo de negocio
/// </summary>
public class Result
{
    public bool Succeeded { get; set; }
    public string[] Errors { get; set; } = Array.Empty<string>();

    public static Result Success() => new() { Succeeded = true };

    public static Result Failure(params string[] errors) => new()
    {
        Succeeded = false,
        Errors = errors
    };
}

/// <summary>
/// Result genérico con datos de respuesta
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; set; }

    public static Result<T> Success(T data) => new()
    {
        Succeeded = true,
        Data = data
    };

    public static new Result<T> Failure(params string[] errors) => new()
    {
        Succeeded = false,
        Errors = errors
    };
}