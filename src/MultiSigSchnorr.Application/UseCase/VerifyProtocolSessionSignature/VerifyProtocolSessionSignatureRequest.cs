namespace MultiSigSchnorr.Application.UseCases.VerifyProtocolSessionSignature;

public sealed class VerifyProtocolSessionSignatureRequest
{
    public Guid SessionId { get; init; }
}