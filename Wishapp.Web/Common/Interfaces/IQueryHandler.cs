using Wishapp.Web.Common.Types;

namespace Wishapp.Web.Common.Interfaces;

public interface IQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> HandleAsync(TQuery query, CancellationToken ct = default);
}