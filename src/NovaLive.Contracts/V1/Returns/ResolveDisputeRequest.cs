namespace NovaLive.Contracts.V1.Returns;

public record ResolveDisputeRequest(
    string Winner,
    decimal? RefundAmount,
    string AdminNote);
