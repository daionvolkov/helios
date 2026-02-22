

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
    private static AppResult Failure(AppError error) => new(false, error);
    public static AppResult Validation(string message, string code = ErrorCodes.Common.ValidationFailed)
        => Failure(new AppError(code, message, ErrorKind.Validation));

    public static AppResult Unauthorized(string message = "Unauthorized", string code = ErrorCodes.Common.Unauthorized)
        => Failure(new AppError(code, message, ErrorKind.Unauthorized));

    public static AppResult Forbidden(string message = "Forbidden", string code = ErrorCodes.Common.Forbidden)
        => Failure(new AppError(code, message, ErrorKind.Forbidden));

    public static AppResult NotFound(string message = "Not found", string code = ErrorCodes.Common.NotFound)
        => Failure(new AppError(code, message, ErrorKind.NotFound));

    public static AppResult Conflict(string message = "Conflict", string code = ErrorCodes.Common.Conflict)
        => Failure(new AppError(code, message, ErrorKind.Conflict));

    public static AppResult Internal(string message = "Unexpected error", string code = ErrorCodes.Common.Unexpected)
        => Failure(new AppError(code, message, ErrorKind.Internal));
}

public partial class AppResult<T> : AppResult
{
    public T? Value { get; }

    private AppResult(bool isSuccess, T? value, AppError? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static AppResult<T> Success(T value) => new(true, value, null);

    public new static AppResult<T> Failure(AppError error) => new(false, default, error);
    
    public new static AppResult<T> Validation(string message, string code = ErrorCodes.Common.ValidationFailed)
        => Failure(new AppError(code, message, ErrorKind.Validation));

    public new static AppResult<T> Unauthorized(string message = "Unauthorized", string code = ErrorCodes.Common.Unauthorized)
        => Failure(new AppError(code, message, ErrorKind.Unauthorized));

    public new static AppResult<T> Forbidden(string message = "Forbidden", string code = ErrorCodes.Common.Forbidden)
        => Failure(new AppError(code, message, ErrorKind.Forbidden));

    public new static AppResult<T> NotFound(string message = "Not found", string code = ErrorCodes.Common.NotFound)
        => Failure(new AppError(code, message, ErrorKind.NotFound));

    public new static AppResult<T> Conflict(string message = "Conflict", string code = ErrorCodes.Common.Conflict)
        => Failure(new AppError(code, message, ErrorKind.Conflict));

    public new static AppResult<T> Internal(string message = "Unexpected error", string code = ErrorCodes.Common.Unexpected)
        => Failure(new AppError(code, message, ErrorKind.Internal));
}
