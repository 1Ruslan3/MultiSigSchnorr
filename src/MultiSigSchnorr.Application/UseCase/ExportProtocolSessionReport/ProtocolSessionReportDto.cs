using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Application.UseCases.ExportProtocolSessionReport;

public sealed class ProtocolSessionReportDto
{
    public Guid SessionId { get; init; }
    public Guid EpochId { get; init; }
    public int EpochNumber { get; init; }

    public SessionStatus SessionStatus { get; init; }

    public DateTime CreatedUtc { get; init; }
    public DateTime? CompletedUtc { get; init; }

    public string MessageDigestHex { get; init; } = string.Empty;
    public string AggregatePublicKeyHex { get; init; } = string.Empty;
    public string? AggregateNoncePointHex { get; init; }
    public string? ChallengeHex { get; init; }
    public string? AggregateSignatureNoncePointHex { get; init; }
    public string? AggregateSignatureScalarHex { get; init; }

    public bool AllCommitmentsPublished { get; init; }
    public bool AllNoncesRevealed { get; init; }
    public bool AllPartialSignaturesSubmitted { get; init; }

    public IReadOnlyList<ProtocolSessionReportParticipantDto> Participants { get; init; }
        = Array.Empty<ProtocolSessionReportParticipantDto>();
}

public sealed class ProtocolSessionReportParticipantDto
{
    public Guid ParticipantId { get; init; }
    public string DisplayName { get; init; } = string.Empty;

    public string PublicKeyHex { get; init; } = string.Empty;
    public string AggregationCoefficientHex { get; init; } = string.Empty;

    public bool HasCommitment { get; init; }
    public bool HasReveal { get; init; }
    public bool HasPartialSignature { get; init; }

    public string? CommitmentHex { get; init; }
    public string? PublicNoncePointHex { get; init; }
    public string? PartialSignatureHex { get; init; }
}