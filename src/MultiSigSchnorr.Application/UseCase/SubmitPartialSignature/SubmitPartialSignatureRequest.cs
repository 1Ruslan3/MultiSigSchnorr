namespace MultiSigSchnorr.Application.UseCases.SubmitPartialSignature;

public sealed class SubmitPartialSignatureRequest
{
    public Guid SessionId { get; init; }
    public Guid ParticipantId { get; init; }
}