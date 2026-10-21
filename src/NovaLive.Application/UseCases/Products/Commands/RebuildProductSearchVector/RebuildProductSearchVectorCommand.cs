using NovaLive.Application.Common.Messaging;

namespace NovaLive.Application.UseCases.Products.Commands.RebuildProductSearchVector;

public sealed record RebuildProductSearchVectorCommand(Guid SpuId) : ICommand, ITransactionalCommand;
