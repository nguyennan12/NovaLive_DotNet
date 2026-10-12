using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Auth;

namespace NovaLive.Application.UseCases.Auth.Commands.Register;

public sealed record RegisterCommand(RegisterUserRequest Data) : ICommand<RegisterUserResponse>, ITransactionalCommand;
