using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Protocol.Epochs;

public sealed class EpochTransitionService
{
    public EpochTransitionResult TransitionToNextEpoch(
        Epoch currentEpoch,
        IReadOnlyList<Participant> participants,
        IReadOnlyList<EpochMember> currentEpochMembers,
        DateTime transitionUtc)
    {
        ArgumentNullException.ThrowIfNull(currentEpoch);
        ArgumentNullException.ThrowIfNull(participants);
        ArgumentNullException.ThrowIfNull(currentEpochMembers);

        if (currentEpoch.Status != EpochStatus.Active)
            throw new InvalidOperationException("Only an active epoch can be transitioned to the next one.");

        var participantMap = participants
            .GroupBy(x => x.Id)
            .ToDictionary(g => g.Key, g => g.First());

        var carriedParticipantIds = new List<Guid>();

        foreach (var member in currentEpochMembers.Where(x => x.EpochId == currentEpoch.Id && x.IsActive))
        {
            if (!participantMap.TryGetValue(member.ParticipantId, out var participant))
                continue;

            if (participant.Status != ParticipantStatus.Active)
                continue;

            carriedParticipantIds.Add(participant.Id);
        }

        carriedParticipantIds = carriedParticipantIds
            .Distinct()
            .ToList();

        if (carriedParticipantIds.Count < 2)
            throw new InvalidOperationException("At least two active participants must remain for the next epoch.");

        currentEpoch.Close(transitionUtc);

        var nextEpoch = new Epoch(
            Guid.NewGuid(),
            currentEpoch.Number + 1,
            transitionUtc);

        nextEpoch.Activate(transitionUtc);

        var newEpochMembers = carriedParticipantIds
            .Select(participantId => new EpochMember(
                Guid.NewGuid(),
                nextEpoch.Id,
                participantId,
                transitionUtc))
            .ToList();

        return new EpochTransitionResult(
            currentEpoch,
            nextEpoch,
            newEpochMembers);
    }
}