using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Contracts.ProtocolSessions;

public sealed class ProtocolSessionHistoryItemApiResponse
{
    public Guid SessionId { get; init; }
    public Guid EpochId { get; init; }
    public int EpochNumber { get; init; }

    public SessionStatus SessionStatus { get; init; }
    public SignatureProtectionMode ProtectionMode { get; init; }

    public DateTime CreatedUtc { get; init; }
    public DateTime? CompletedUtc { get; init; }

    public int ParticipantsCount { get; init; }

    public bool AllCommitmentsPublished { get; init; }
    public bool AllNoncesRevealed { get; init; }
    public bool AllPartialSignaturesSubmitted { get; init; }
}