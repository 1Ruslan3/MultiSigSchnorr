using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Application.UseCases.GetEpochAdministrationState;

public sealed class GetEpochAdministrationStateHandler
{
    private readonly IEpochRepository _epochRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IEpochMemberRepository _epochMemberRepository;
    private readonly IPrivateKeyMaterialRepository _privateKeyMaterialRepository;

    public GetEpochAdministrationStateHandler(
        IEpochRepository epochRepository,
        IParticipantRepository participantRepository,
        IEpochMemberRepository epochMemberRepository,
        IPrivateKeyMaterialRepository privateKeyMaterialRepository)
    {
        _epochRepository = epochRepository ?? throw new ArgumentNullException(nameof(epochRepository));
        _participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
        _epochMemberRepository = epochMemberRepository ?? throw new ArgumentNullException(nameof(epochMemberRepository));
        _privateKeyMaterialRepository = privateKeyMaterialRepository ?? throw new ArgumentNullException(nameof(privateKeyMaterialRepository));
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

        var epochMembers = await _epochMemberRepository.GetByEpochIdAsync(
            activeEpoch.Id,
            cancellationToken);

        var memberMap = epochMembers
            .GroupBy(x => x.ParticipantId)
            .ToDictionary(x => x.Key, x => x.First());

        var participants = await _participantRepository.ListAsync(cancellationToken);

        var participantItems = new List<EpochAdministrationParticipantItemDto>();

        foreach (var participant in participants.OrderBy(x => x.DisplayName, StringComparer.Ordinal))
        {
            var isMember = memberMap.TryGetValue(participant.Id, out var member);
            var isEpochMemberActive = isMember && member!.IsActive;
            var isParticipantActive = participant.Status == ParticipantStatus.Active;
            var hasRuntimePrivateKeyMaterial = await _privateKeyMaterialRepository.HasPrivateKeyMaterialAsync(
                participant.Id,
                cancellationToken);

            var isActiveMember = isMember && isEpochMemberActive && isParticipantActive;

            participantItems.Add(new EpochAdministrationParticipantItemDto
            {
                ParticipantId = participant.Id,
                DisplayName = participant.DisplayName,
                ParticipantStatus = participant.Status,
                PublicKeyHex = participant.PublicKey.ToHex(),
                IsMemberOfActiveEpoch = isMember,
                IsActiveMemberOfActiveEpoch = isActiveMember,
                HasRuntimePrivateKeyMaterial = hasRuntimePrivateKeyMaterial,
                CanBeRevoked = isActiveMember
            });
        }

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
