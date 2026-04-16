using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Application.UseCases.GetEpochAdministrationState;

public sealed class EpochAdministrationStateDto
{
    public Guid ActiveEpochId { get; init; }
    public int ActiveEpochNumber { get; init; }
    public EpochStatus ActiveEpochStatus { get; init; }

    public IReadOnlyList<EpochAdministrationEpochItemDto> Epochs { get; init; }
        = Array.Empty<EpochAdministrationEpochItemDto>();

    public IReadOnlyList<EpochAdministrationParticipantItemDto> Participants { get; init; }
        = Array.Empty<EpochAdministrationParticipantItemDto>();
}

public sealed class EpochAdministrationEpochItemDto
{
    public Guid EpochId { get; init; }
    public int EpochNumber { get; init; }
    public EpochStatus EpochStatus { get; init; }
}

public sealed class EpochAdministrationParticipantItemDto
{
    public Guid ParticipantId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public ParticipantStatus ParticipantStatus { get; init; }
    public string PublicKeyHex { get; init; } = string.Empty;

    public bool IsMemberOfActiveEpoch { get; init; }
    public bool IsActiveMemberOfActiveEpoch { get; init; }
    public bool CanBeRevoked { get; init; }
}