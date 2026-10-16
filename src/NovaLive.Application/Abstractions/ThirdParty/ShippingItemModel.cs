namespace NovaLive.Application.Abstractions.ThirdParty;

public sealed record ShippingItemModel(
    string Name,
    string Code,
    int Quantity,
    int WeightGram);
