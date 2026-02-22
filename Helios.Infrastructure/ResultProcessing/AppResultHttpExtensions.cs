using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace Helios.Infrastructure.ResultProcessing;

public static class AppResultHttpExtensions
{
    public static IActionResult ToActionResult(this AppResult result, HttpContext http)
    {
        if (result.IsSuccess) return new OkResult();
        return CreateProblem(result.Error ?? new AppError(ErrorCodes.Common.Unexpected, "Unknown error", ErrorKind.Internal), http);
    }

    public static IActionResult ToActionResult<T>(this AppResult<T> result, HttpContext http)
    {
        return result.IsSuccess ? new OkObjectResult(result.Value) 
            : CreateProblem(result.Error ?? new AppError(ErrorCodes.Common.Unexpected, "Unknown error", ErrorKind.Internal), http);
    }

    private static ObjectResult CreateProblem(AppError error, HttpContext http)
    {
        var status = error.Kind.ToHttpStatus();

        var pd = new ProblemDetails
        {
            Status = status,
            Title = error.Kind.ToString(),
            Detail = error.Message,
            Type = $"urn:helios:error:{error.Code}"
        };
        
        return new ObjectResult(pd) { StatusCode = status };
    }
}