using NovaLive.Application.Common.Messaging;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Contracts.V1.Auth;

namespace NovaLive.Application.UseCases.Auth.Queries.GetCurrentUser;

[RequireAuthenticated]
public sealed record GetCurrentUserQuery : IQuery<UserInfoResponse>;
