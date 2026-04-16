using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Application.UseCases.GetEpochAdministrationState;

public sealed class GetEpochAdministrationStateHandler
{
    private readonly IEpochRepository _epochRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IEpochMemberRepository _epochMemberRepository;

    public GetEpochAdministrationStateHandler(
        IEpochRepository epochRepository,
        IParticipantRepository participantRepository,
        IEpochMemberRepository epochMemberRepository)
    {
        _epochRepository = epochRepository ?? throw new ArgumentNullException(nameof(epochRepository));
        _participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
        _epochMemberRepository = epochMemberRepository ?? throw new ArgumentNullException(nameof(epochMemberRepository));
    }

    public async Task<EpochAdministrationStateDto> HandleAsync(
        GetEpochAdministrationStateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var epochs = await _epochRepository.ListAsync(cancellationToken);
        var activeEpoch = epochs
            .Where(x => x.Status == EpochStatus.Active)
            .OrderByDescending(x => x.Number)
            .FirstOrDefault();

        if (activeEpoch is null)
            throw new InvalidOperationException("Active epoch was not found.");

        var epochMembers = await _epochMemberRepository.GetByEpochIdAsync(activeEpoch.Id, cancellationToken);
        var memberMap = epochMembers
            .GroupBy(x => x.ParticipantId)
            .ToDictionary(x => x.Key, x => x.First());

        var participants = await _participantRepository.ListAsync(cancellationToken);

        var participantItems = participants
            .OrderBy(x => x.DisplayName, StringComparer.Ordinal)
            .Select(x =>
            {
                var isMember = memberMap.TryGetValue(x.Id, out var member);
                var isActiveMember = isMember && member!.IsActive;

                return new EpochAdministrationParticipantItemDto
                {
                    ParticipantId = x.Id,
                    DisplayName = x.DisplayName,
                    ParticipantStatus = x.Status,
                    PublicKeyHex = x.PublicKey.ToHex(),
                    IsMemberOfActiveEpoch = isMember,
                    IsActiveMemberOfActiveEpoch = isActiveMember,
                    CanBeRevoked = x.Status == ParticipantStatus.Active && isActiveMember
                };
            })
            .ToList();

        var epochItems = epochs
            .OrderByDescending(x => x.Number)
            .Select(x => new EpochAdministrationEpochItemDto
            {
                EpochId = x.Id,
                EpochNumber = x.Number,
                EpochStatus = x.Status
            })
            .ToList();

        return new EpochAdministrationStateDto
        {
            ActiveEpochId = activeEpoch.Id,
            ActiveEpochNumber = activeEpoch.Number,
            ActiveEpochStatus = activeEpoch.Status,
            Epochs = epochItems,
            Participants = participantItems
        };
    }
}