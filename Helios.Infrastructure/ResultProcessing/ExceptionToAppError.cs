namespace Helios.Infrastructure.ResultProcessing;

public static class ExceptionToAppError
{
    public static AppError ToAppError(this Exception ex)
    {
        return ex switch
        {
            OperationCanceledException => new AppError(ErrorCodes.Common.Cancelled, "Request was cancelled", ErrorKind.Internal),
            ArgumentException ae       => new AppError(ErrorCodes.Common.ValidationFailed, ae.Message, ErrorKind.Validation),
            KeyNotFoundException       => new AppError(ErrorCodes.Common.NotFound, "Not found", ErrorKind.NotFound),
            _ => new AppError(ErrorCodes.Common.Unexpected, "Unexpected error", ErrorKind.Internal),
        };
    }
}