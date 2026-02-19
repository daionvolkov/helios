namespace Helios.Platform.Common;

public class DomainResult
{
    public bool IsSuccess { get; }
    public DomainError? Error { get; }

    protected DomainResult(bool isSuccess, DomainError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static DomainResult Success() => new(true, null);

    public static DomainResult Failure(DomainError error) => new(false, error);
}

public sealed class DomainResult<T> : DomainResult
{
    public T? Value { get; }

    private DomainResult(bool isSuccess, T? value, DomainError? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static DomainResult<T> Success(T value) => new(true, value, null);

    public static new DomainResult<T> Failure(DomainError error) => new(false, default, error);
}

