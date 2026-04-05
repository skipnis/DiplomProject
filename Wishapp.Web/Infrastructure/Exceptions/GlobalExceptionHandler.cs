using Microsoft.AspNetCore.Diagnostics;

namespace Wishapp.Web.Infrastructure.Exceptions;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException)
        {
            return true;
        }

        logger.LogError(exception, "Unhandled exception: {ExceptionMessage}", exception.Message);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Title = "Internal Server Error",
                Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1"
            }
        });
    }
}
