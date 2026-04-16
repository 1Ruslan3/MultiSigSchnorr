namespace MultiSigSchnorr.Application.UseCases.RevokeParticipantInActiveEpoch;

public sealed class RevokeParticipantInActiveEpochRequest
{
    public Guid ParticipantId { get; init; }
    public string Reason { get; init; } = string.Empty;
}