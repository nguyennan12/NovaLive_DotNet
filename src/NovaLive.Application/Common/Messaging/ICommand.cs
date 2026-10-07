using MediatR;
using NovaLive.Domain.Common;

namespace NovaLive.Application.Common.Messaging;

public interface IBaseCommand;

public interface ICommand : IRequest<Result>, IBaseCommand;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;
