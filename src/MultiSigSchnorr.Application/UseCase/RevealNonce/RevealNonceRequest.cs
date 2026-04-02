namespace MultiSigSchnorr.Application.UseCases.RevealNonce;

public sealed class RevealNonceRequest
{
    public Guid SessionId { get; init; }
    public Guid ParticipantId { get; init; }
}