namespace Helios.Infrastructure.ResultProcessing;

public static class ToAppErrorExtensions
{
    public static AppError ToAppError(this AppError err) => err;
    
    public static AppError ToAppError(this object err)
    {
        switch (err)
        {
            case AppError ae:
                return ae;
            case IHasErrorInfo info:
                return Map(info.Code, info.Message, info.Kind);
        }

        var code = TryGetString(err, "Code");
        var message = TryGetString(err, "Message") ?? "Unexpected error";
        var kind = TryGetString(err, "Kind") ?? TryGetString(err, "Type"); // иногда называется Type

        if (!string.IsNullOrWhiteSpace(code))
            return Map(code!, message!, kind);

        return new AppError(
            ErrorCodes.Common.Unexpected,
            "Unexpected error",
            ErrorKind.Internal);
    }
    
    public static AppError ToAppError(this Exception ex)
        => ex switch
        {
            ArgumentException a =>
                new AppError(ErrorCodes.Common.ValidationFailed, a.Message, ErrorKind.Validation),

            UnauthorizedAccessException =>
                new AppError(ErrorCodes.Common.Unauthorized, "Unauthorized", ErrorKind.Unauthorized),

            KeyNotFoundException =>
                new AppError(ErrorCodes.Common.NotFound, "Not found", ErrorKind.NotFound),

            InvalidOperationException i =>
                new AppError(ErrorCodes.Common.Conflict, i.Message, ErrorKind.Conflict),

            OperationCanceledException =>
                new AppError(ErrorCodes.Common.Cancelled, "Request was cancelled", ErrorKind.Internal),

            _ =>
                new AppError(ErrorCodes.Common.Unexpected, "Unexpected error", ErrorKind.Internal)
        };

    private static AppError Map(string code, string message, string? kindText)
    {
        var kind = ParseKind(kindText, code);

        
        if (string.IsNullOrWhiteSpace(code))
            code = ErrorCodes.Common.Unexpected;
        
        if (string.IsNullOrWhiteSpace(message))
            message = "Unexpected error";

        return new AppError(code, message, kind);
    }

    private static ErrorKind ParseKind(string? kindText, string code)
    {
        if (!string.IsNullOrWhiteSpace(kindText) &&
            Enum.TryParse<ErrorKind>(kindText, ignoreCase: true, out var parsed))
            return parsed;

        if (code.StartsWith("auth.", StringComparison.OrdinalIgnoreCase))
            return code switch
            {
                ErrorCodes.Auth.InvalidCredentials => ErrorKind.Unauthorized,
                ErrorCodes.Auth.UserInactive => ErrorKind.Forbidden,
                _ => ErrorKind.Unauthorized
            };

        if (code.EndsWith(".not_found", StringComparison.OrdinalIgnoreCase))
            return ErrorKind.NotFound;

        if (code.EndsWith(".forbidden", StringComparison.OrdinalIgnoreCase))
            return ErrorKind.Forbidden;

        if (code.EndsWith(".conflict", StringComparison.OrdinalIgnoreCase))
            return ErrorKind.Conflict;

        if (code.Contains("validation", StringComparison.OrdinalIgnoreCase) ||
            code.EndsWith(".validation_failed", StringComparison.OrdinalIgnoreCase))
            return ErrorKind.Validation;
        
        return ErrorKind.Internal;
    }

    private static string? TryGetString(object obj, string propName)
    {
        var p = obj.GetType().GetProperty(propName);
        if (p == null) return null;
        if (p.PropertyType != typeof(string) && p.PropertyType != typeof(object)) return null;

        return p.GetValue(obj) as string;
    }
}