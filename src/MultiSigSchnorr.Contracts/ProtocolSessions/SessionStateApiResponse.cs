using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Contracts.ProtocolSessions;

public sealed class SessionStateApiResponse
{
    public Guid SessionId { get; init; }
    public Guid EpochId { get; init; }
    public int EpochNumber { get; init; }
    public SessionStatus SessionStatus { get; init; }
    public SignatureProtectionMode ProtectionMode { get; init; }

    public string MessageDigestHex { get; init; } = string.Empty;
    public string AggregatePublicKeyHex { get; init; } = string.Empty;
    public string? AggregateNoncePointHex { get; init; }
    public string? ChallengeHex { get; init; }
    public string? AggregateSignatureNoncePointHex { get; init; }
    public string? AggregateSignatureScalarHex { get; init; }

    public bool AllCommitmentsPublished { get; init; }
    public bool AllNoncesRevealed { get; init; }
    public bool AllPartialSignaturesSubmitted { get; init; }

    public IReadOnlyList<SessionParticipantStateApiResponse> Participants { get; init; }
        = Array.Empty<SessionParticipantStateApiResponse>();
}

public sealed class SessionParticipantStateApiResponse
{
    public Guid ParticipantId { get; init; }
    public string DisplayName { get; init; } = string.Empty;

    public bool HasCommitment { get; init; }
    public bool HasReveal { get; init; }
    public bool HasPartialSignature { get; init; }

    public string PublicKeyHex { get; init; } = string.Empty;
    public string AggregationCoefficientHex { get; init; } = string.Empty;

    public string? CommitmentHex { get; init; }
    public string? PublicNoncePointHex { get; init; }
    public string? PartialSignatureHex { get; init; }
}