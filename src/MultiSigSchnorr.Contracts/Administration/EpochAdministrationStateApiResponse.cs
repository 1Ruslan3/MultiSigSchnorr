using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Contracts.Administration;

public sealed class EpochAdministrationStateApiResponse
{
    public Guid ActiveEpochId { get; init; }
    public int ActiveEpochNumber { get; init; }
    public EpochStatus ActiveEpochStatus { get; init; }

    public IReadOnlyList<EpochAdministrationEpochApiResponse> Epochs { get; init; }
        = Array.Empty<EpochAdministrationEpochApiResponse>();

    public IReadOnlyList<EpochAdministrationParticipantApiResponse> Participants { get; init; }
        = Array.Empty<EpochAdministrationParticipantApiResponse>();
}

public sealed class EpochAdministrationEpochApiResponse
{
    public Guid EpochId { get; init; }
    public int EpochNumber { get; init; }
    public EpochStatus EpochStatus { get; init; }
}

public sealed class EpochAdministrationParticipantApiResponse
{
    public Guid ParticipantId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public ParticipantStatus ParticipantStatus { get; init; }
    public string PublicKeyHex { get; init; } = string.Empty;

    public bool IsMemberOfActiveEpoch { get; init; }
    public bool IsActiveMemberOfActiveEpoch { get; init; }
    public bool CanBeRevoked { get; init; }
}