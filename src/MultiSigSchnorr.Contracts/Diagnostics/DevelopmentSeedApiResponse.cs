namespace MultiSigSchnorr.Contracts.Diagnostics;

public sealed class DevelopmentSeedApiResponse
{
    public Guid EpochId { get; init; }
    public int EpochNumber { get; init; }

    public Guid Participant1Id { get; init; }
    public Guid Participant2Id { get; init; }
    public Guid Participant3Id { get; init; }

    public IReadOnlyList<Guid> ParticipantIds { get; init; } = Array.Empty<Guid>();
}