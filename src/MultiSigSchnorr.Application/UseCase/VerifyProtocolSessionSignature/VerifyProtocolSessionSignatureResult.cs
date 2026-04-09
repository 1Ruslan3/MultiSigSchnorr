namespace MultiSigSchnorr.Application.UseCases.VerifyProtocolSessionSignature;

public sealed class VerifyProtocolSessionSignatureResult
{
    public Guid SessionId { get; init; }
    public bool IsValid { get; init; }
    public string Message { get; init; } = string.Empty;
}