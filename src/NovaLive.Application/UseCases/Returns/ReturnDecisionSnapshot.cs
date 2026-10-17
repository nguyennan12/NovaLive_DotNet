using NovaLive.Contracts.V1.Shops;
namespace NovaLive.Application.UseCases.Returns;
public sealed record ReturnDecisionSnapshot(string? Note, IReadOnlyList<string> EvidenceUrls, ShopAddressResponse? ReturnAddress);
