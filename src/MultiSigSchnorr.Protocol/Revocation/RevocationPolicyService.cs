using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Protocol.Revocation;

public sealed class RevocationPolicyService
{
    public RevocationOperationResult RevokeParticipant(
        Epoch epoch,
        Participant participant,
        IReadOnlyList<EpochMember> epochMembers,
        string reason,
        DateTime revokedUtc)
    {
        ArgumentNullException.ThrowIfNull(epoch);
        ArgumentNullException.ThrowIfNull(participant);
        ArgumentNullException.ThrowIfNull(epochMembers);

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Revocation reason cannot be empty.", nameof(reason));

        if (epoch.Status != EpochStatus.Active)
            throw new InvalidOperationException("Participant revocation is allowed only in an active epoch.");

        if (participant.Status == ParticipantStatus.Revoked)
            throw new InvalidOperationException("Participant has already been revoked.");

        var affectedMembers = epochMembers
            .Where(x => x.EpochId == epoch.Id && x.ParticipantId == participant.Id && x.IsActive)
            .ToList();

        if (affectedMembers.Count == 0)
            throw new InvalidOperationException("Participant does not have an active membership in the specified epoch.");

        foreach (var member in affectedMembers)
            member.Deactivate();

        participant.Revoke(revokedUtc);

        var record = new RevocationRecord(
            Guid.NewGuid(),
            participant.Id,
            epoch.Id,
            reason.Trim(),
            revokedUtc);

        return new RevocationOperationResult(
            participant,
            record,
            affectedMembers);
    }
}