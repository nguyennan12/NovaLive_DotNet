namespace NovaLive.Contracts.V1.Returns;
public sealed record RejectReturnRequest(string Reason, IReadOnlyList<string> EvidenceUrls);
