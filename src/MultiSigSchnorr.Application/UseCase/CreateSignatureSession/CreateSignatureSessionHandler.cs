using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Application.UseCases.CreateSignatureSession;

public sealed class CreateSignatureSessionHandler
{
    public SignatureSession Handle(
        CreateSignatureSessionRequest request,
        Epoch epoch,
        IReadOnlyList<Participant> participants,
        IReadOnlyList<EpochMember> members,
        DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(epoch);

        if (epoch.Status != EpochStatus.Active)
            throw new InvalidOperationException("Epoch must be active.");

        var memberSet = members
            .Where(x => x.EpochId == epoch.Id && x.IsActive)
            .Select(x => x.ParticipantId)
            .ToHashSet();

        var participantMap = participants.ToDictionary(x => x.Id);

        foreach (var id in request.ParticipantIds)
        {
            if (!memberSet.Contains(id))
                throw new InvalidOperationException($"Participant {id} is not in active epoch.");

            if (!participantMap.TryGetValue(id, out var p))
                throw new InvalidOperationException($"Participant {id} not found.");

            if (p.Status != ParticipantStatus.Active)
                throw new InvalidOperationException($"Participant {id} is not active.");
        }

        var session = new SignatureSession(
            Guid.NewGuid(),
            epoch.Id,
            request.ParticipantIds,
            request.Message,
            nowUtc);

        return session;
    }
}