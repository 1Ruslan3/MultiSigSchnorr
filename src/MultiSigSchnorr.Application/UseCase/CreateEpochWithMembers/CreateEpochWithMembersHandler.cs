using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Application.UseCases.CreateEpochWithMembers;

public sealed class CreateEpochWithMembersHandler
{
    private readonly IEpochRepository _epochRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IEpochMemberRepository _epochMemberRepository;

    public CreateEpochWithMembersHandler(
        IEpochRepository epochRepository,
        IParticipantRepository participantRepository,
        IEpochMemberRepository epochMemberRepository)
    {
        _epochRepository = epochRepository ?? throw new ArgumentNullException(nameof(epochRepository));
        _participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
        _epochMemberRepository = epochMemberRepository ?? throw new ArgumentNullException(nameof(epochMemberRepository));
    }

    public async Task<Epoch> HandleAsync(
        CreateEpochWithMembersRequest request,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var participantIds = request.ParticipantIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (participantIds.Count < 2)
            throw new InvalidOperationException("At least two active participants are required for a new epoch.");

        var participants = await _participantRepository.GetByIdsAsync(
            participantIds,
            cancellationToken);

        if (participants.Count != participantIds.Count)
            throw new InvalidOperationException("One or more selected participants were not found.");

        var inactiveParticipants = participants
            .Where(x => x.Status != ParticipantStatus.Active)
            .Select(x => $"{x.DisplayName} ({x.Id})")
            .ToList();

        if (inactiveParticipants.Count > 0)
        {
            throw new InvalidOperationException(
                "Only active participants can be included in a new epoch. Invalid participants: " +
                string.Join(", ", inactiveParticipants));
        }

        var epochs = await _epochRepository.ListAsync(cancellationToken);

        foreach (var activeEpoch in epochs.Where(x => x.Status == EpochStatus.Active))
        {
            activeEpoch.Close(nowUtc);
            await _epochRepository.UpdateAsync(activeEpoch, cancellationToken);
        }

        var nextEpochNumber = epochs.Count == 0
            ? 1
            : epochs.Max(x => x.Number) + 1;

        var epoch = new Epoch(
            Guid.NewGuid(),
            nextEpochNumber,
            nowUtc);

        epoch.Activate(nowUtc);

        await _epochRepository.AddAsync(epoch, cancellationToken);

        foreach (var participantId in participantIds)
        {
            await _epochMemberRepository.AddAsync(
                new EpochMember(
                    Guid.NewGuid(),
                    epoch.Id,
                    participantId,
                    nowUtc),
                cancellationToken);
        }

        return epoch;
    }
}