namespace MultiSigSchnorr.Contracts.Administration;

public sealed class CreateEpochWithMembersApiRequest
{
    public IReadOnlyList<Guid> ParticipantIds { get; init; } = Array.Empty<Guid>();
}