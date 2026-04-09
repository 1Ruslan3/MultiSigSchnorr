namespace MultiSigSchnorr.Contracts.ProtocolSessions;

public sealed class VerifyProtocolSessionSignatureApiRequest
{
    public Guid SessionId { get; init; }
}