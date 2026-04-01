namespace MultiSigSchnorr.Application.UseCases.CreateSignatureSession;

public sealed class CreateSignatureSessionRequest
{
    public Guid EpochId { get; init; }
    public IReadOnlyList<Guid> ParticipantIds { get; init; } = default!;
    public byte[] Message { get; init; } = default!;
}