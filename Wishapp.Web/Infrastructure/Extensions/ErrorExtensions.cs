using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Infrastructure.Extensions;

public static class ErrorExtensions
{
    public static IResult ToHttpResult(this Error error) =>
        error.Type switch
        {
            ErrorType.NotFound => TypedResults.NotFound(error),
            ErrorType.Conflict => TypedResults.Conflict(error),
            ErrorType.Validation => TypedResults.UnprocessableEntity(error),
            ErrorType.Forbidden => TypedResults.Forbid(),
            _ => TypedResults.Problem(error.Description)
        };
}