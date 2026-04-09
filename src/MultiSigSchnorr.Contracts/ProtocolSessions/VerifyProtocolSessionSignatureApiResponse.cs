namespace MultiSigSchnorr.Contracts.ProtocolSessions;

public sealed class VerifyProtocolSessionSignatureApiResponse
{
    public Guid SessionId { get; init; }
    public bool IsValid { get; init; }
    public string Message { get; init; } = string.Empty;
}