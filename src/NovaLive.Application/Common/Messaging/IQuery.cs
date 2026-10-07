using MediatR;
using NovaLive.Domain.Common;

namespace NovaLive.Application.Common.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
