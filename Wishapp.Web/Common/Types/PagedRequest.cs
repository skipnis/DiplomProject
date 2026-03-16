using System.ComponentModel.DataAnnotations;

namespace Wishapp.Web.Common.Types;

public record PagedRequest(
    int Page = 1,
    [property: Range(1, 100)] int PageSize = 20);