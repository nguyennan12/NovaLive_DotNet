using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Returns;
using NovaLive.Domain.Rbac;
namespace NovaLive.Application.UseCases.Returns.Queries.GetReturnDetail;
public sealed record GetReturnDetailQuery(Guid Id) : IQuery<OrderReturnResponse>, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Returns.RequestOwn;
}
