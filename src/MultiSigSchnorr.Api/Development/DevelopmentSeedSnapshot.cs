namespace MultiSigSchnorr.Api.Development;

public sealed class DevelopmentSeedSnapshot
{
    public Guid EpochId { get; init; }
    public int EpochNumber { get; init; }

    public Guid Participant1Id { get; init; }
    public Guid Participant2Id { get; init; }
    public Guid Participant3Id { get; init; }

    public IReadOnlyList<Guid> ParticipantIds => new[]
    {
        Participant1Id,
        Participant2Id,
        Participant3Id
    };
}