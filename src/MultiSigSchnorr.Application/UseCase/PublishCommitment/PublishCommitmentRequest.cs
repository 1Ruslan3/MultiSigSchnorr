namespace MultiSigSchnorr.Application.UseCases.PublishCommitment;

public sealed class PublishCommitmentRequest
{
    public Guid SessionId { get; init; }
    public Guid ParticipantId { get; init; }
}