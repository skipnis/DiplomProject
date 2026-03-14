namespace Wishapp.Web.Common.Types;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, Error.None);
    public static Result<T> Failure<T>(Error error) => new(default!, false, error);
}

public sealed class Result<T> : Result
{
    internal Result(T value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        Value = value;
    }

    public T Value => IsSuccess
        ? field
        : throw new InvalidOperationException("Cannot access Value of a failed result.");

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure<T>(error);
}