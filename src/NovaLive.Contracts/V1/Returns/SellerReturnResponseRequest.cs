namespace NovaLive.Contracts.V1.Returns;

public record SellerReturnResponseRequest(
    [property: System.Text.Json.Serialization.JsonPropertyName("note")] string? ReasonNote);
