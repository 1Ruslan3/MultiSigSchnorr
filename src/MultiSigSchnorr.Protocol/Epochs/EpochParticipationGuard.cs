using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Protocol.Epochs;

public sealed class EpochParticipationGuard
{
    public void EnsureSessionCanBeCreated(
        Epoch epoch,
        IReadOnlyList<Participant> participants,
        IReadOnlyList<EpochMember> epochMembers)
    {
        ArgumentNullException.ThrowIfNull(epoch);
        ArgumentNullException.ThrowIfNull(participants);
        ArgumentNullException.ThrowIfNull(epochMembers);

        if (epoch.Status != EpochStatus.Active)
            throw new InvalidOperationException("Signing session can only be created for an active epoch.");

        if (participants.Count < 2)
            throw new InvalidOperationException("At least two active participants are required.");

        var activeEpochMemberIds = epochMembers
            .Where(x => x.EpochId == epoch.Id && x.IsActive)
            .Select(x => x.ParticipantId)
            .ToHashSet();

        foreach (var participant in participants)
        {
            if (participant.Status != ParticipantStatus.Active)
                throw new InvalidOperationException(
                    $"Participant '{participant.DisplayName}' is not active and cannot join the signing session.");

            if (!activeEpochMemberIds.Contains(participant.Id))
                throw new InvalidOperationException(
                    $"Participant '{participant.DisplayName}' does not belong to the active membership of the epoch.");
        }
    }
}