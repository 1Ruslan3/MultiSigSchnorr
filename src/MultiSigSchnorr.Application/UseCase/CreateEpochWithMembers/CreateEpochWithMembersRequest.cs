namespace MultiSigSchnorr.Application.UseCases.CreateEpochWithMembers;

public sealed class CreateEpochWithMembersRequest
{
    public IReadOnlyList<Guid> ParticipantIds { get; init; } = Array.Empty<Guid>();
}