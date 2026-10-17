using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Returns;
using NovaLive.Domain.Rbac;
namespace NovaLive.Application.UseCases.Returns.Queries.GetMyReturns;
public sealed record GetMyReturnsQuery(string? Status = null, int Page = 1, int Size = 20) : IQuery<PagedResult<OrderReturnResponse>>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Returns.RequestOwn;
}
