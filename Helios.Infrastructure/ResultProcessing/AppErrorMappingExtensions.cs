namespace Helios.Infrastructure.ResultProcessing;

public static class AppErrorMappingExtensions
{
    public static AppError ToAppError(this Exception ex)
    {
        return ex switch
        {
            ArgumentException arg =>
                new AppError(
                    ErrorCodes.Common.ValidationFailed,
                    arg.Message,
                    ErrorKind.Validation),

            UnauthorizedAccessException =>
                new AppError(
                    ErrorCodes.Common.Unauthorized,
                    "Unauthorized",
                    ErrorKind.Unauthorized),

            KeyNotFoundException =>
                new AppError(
                    ErrorCodes.Common.NotFound,
                    "Resource not found",
                    ErrorKind.NotFound),

            InvalidOperationException inv =>
                new AppError(
                    ErrorCodes.Common.Conflict,
                    inv.Message,
                    ErrorKind.Conflict),

            OperationCanceledException =>
                new AppError(
                    ErrorCodes.Common.Cancelled,
                    "Operation cancelled",
                    ErrorKind.Internal),

            _ =>
                new AppError(
                    ErrorCodes.Common.Unexpected,
                    "Unexpected error",
                    ErrorKind.Internal)
        };
    }
}