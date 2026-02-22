using Microsoft.AspNetCore.Http;

namespace Helios.Infrastructure.ResultProcessing;

public static class ErrorKindHttpMapping
{
    public static int ToHttpStatus(this ErrorKind kind) => kind switch
    {
        ErrorKind.Validation   => StatusCodes.Status400BadRequest,
        ErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorKind.Forbidden    => StatusCodes.Status403Forbidden,
        ErrorKind.NotFound     => StatusCodes.Status404NotFound,
        ErrorKind.Conflict     => StatusCodes.Status409Conflict,
        ErrorKind.Internal     => StatusCodes.Status500InternalServerError,
        _                      => StatusCodes.Status500InternalServerError
    };
}