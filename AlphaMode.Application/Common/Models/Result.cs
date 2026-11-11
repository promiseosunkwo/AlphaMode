namespace AlphaMode.Application.Common.Models;

public class Result
{
    public bool Succeeded { get; init; }
    public string? Message { get; init; }
    public string? Error { get; init; }

    public static Result Success(string? message = null)
    {
        return new Result
        {
            Succeeded = true,
            Message = message
        };
    }

    public static Result Failure(string error)
    {
        return new Result
        {
            Succeeded = false,
            Error = error
        };
    }
}

public class Result<T>
{
    public bool Succeeded { get; init; }
    public T? Value { get; init; }
    public string? Message { get; init; }
    public string? Error { get; init; }

    public static Result<T> Success(
        T value,
        string? message = null)
    {
        return new Result<T>
        {
            Succeeded = true,
            Value = value,
            Message = message
        };
    }

    public static Result<T> Failure(string error)
    {
        return new Result<T>
        {
            Succeeded = false,
            Error = error
        };
    }
}