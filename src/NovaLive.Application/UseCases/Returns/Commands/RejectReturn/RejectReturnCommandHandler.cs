using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Returns;
using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Returns.Commands.RejectReturn;
public sealed class RejectReturnCommandHandler(SellerReturnAccess access, ReturnReader reader, IDateTimeProvider clock)
    : ICommandHandler<RejectReturnCommand, OrderReturnResponse>
{
    public async Task<Result<OrderReturnResponse>> Handle(RejectReturnCommand command, CancellationToken ct)
    {
        var loaded = await access.GetForUpdateAsync(command.Id, ct);
        if (loaded.IsFailure) return loaded.Error;
        var entity = loaded.Value!.Request;
        var result = entity.Reject(ReturnMappings.SerializeDecision(new(command.Request.Reason, command.Request.EvidenceUrls, null)), clock.UtcNow);
        if (result.IsFailure) return result.Error;
        return (await reader.DetailAsync(entity, ct))!;
    }
}
