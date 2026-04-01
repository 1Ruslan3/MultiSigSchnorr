using MultiSigSchnorr.Domain.Entities;

namespace MultiSigSchnorr.Protocol.Revocation;

public sealed class RevocationOperationResult
{
    private readonly IReadOnlyList<EpochMember> _deactivatedMembers;

    public Participant Participant { get; }
    public RevocationRecord RevocationRecord { get; }
    public IReadOnlyList<EpochMember> DeactivatedMembers => _deactivatedMembers;

    public RevocationOperationResult(
        Participant participant,
        RevocationRecord revocationRecord,
        IReadOnlyList<EpochMember> deactivatedMembers)
    {
        Participant = participant ?? throw new ArgumentNullException(nameof(participant));
        RevocationRecord = revocationRecord ?? throw new ArgumentNullException(nameof(revocationRecord));
        _deactivatedMembers = deactivatedMembers ?? throw new ArgumentNullException(nameof(deactivatedMembers));
    }
}