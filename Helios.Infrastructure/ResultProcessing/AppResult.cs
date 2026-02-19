

namespace Helios.Infrastructure.ResultProcessing;

public class AppResult
{
    public bool IsSuccess { get; }
    public AppError? Error { get; }

    protected AppResult(bool isSuccess, AppError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static AppResult Success() => new(true, null);

    public static AppResult Failure(AppError error) => new(false, error);

}

public sealed class AppResult<T> : AppResult
{
    public T? Value { get; }

    private AppResult(bool isSuccess, T? value, AppError? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static AppResult<T> Success(T value) => new(true, value, null);

    public static new AppResult<T> Failure(AppError error) => new(false, default, error);
}
