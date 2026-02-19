
using Helios.Infrastructure.ResultProcessing;
using Microsoft.AspNetCore.Mvc;

namespace Helios.Api.Common;

public static class AppResultExtensions
{
    public static IActionResult ToActionResult(this AppResult result, ControllerBase controller)
    {
        if (result.IsSuccess)
            return controller.Ok();

        return controller.ProblemFromError(result.Error);
    }

    public static IActionResult ToActionResult<T>(this AppResult<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
            return controller.Ok(result.Value);

        return controller.ProblemFromError(result.Error);
    }

    private static IActionResult ProblemFromError(this ControllerBase controller, AppError? error)
    {
        // Fallback safety
        if (error == null)
        {
            return controller.Problem(
                title: "Unexpected error",
                detail: "No error information was provided.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var status = error.Kind switch
        {
            ErrorKind.Validation => StatusCodes.Status400BadRequest,
            ErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorKind.Forbidden => StatusCodes.Status403Forbidden,
            ErrorKind.NotFound => StatusCodes.Status404NotFound,
            ErrorKind.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        // ProblemDetails response; clients can rely on "extensions.code"
        var pd = new ProblemDetails
        {
            Status = status,
            Title = error.Kind.ToString(),
            Detail = error.Message,
            Type = $"urn:helios:error:{error.Code}"
        };

        pd.Extensions["code"] = error.Code;

        return controller.Problem(
            title: pd.Title,
            detail: pd.Detail,
            statusCode: pd.Status,
            type: pd.Type);
    }
}
