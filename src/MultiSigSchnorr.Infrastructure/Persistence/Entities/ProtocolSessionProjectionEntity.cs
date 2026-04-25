namespace MultiSigSchnorr.Infrastructure.Persistence.Entities;

public sealed class ProtocolSessionProjectionEntity
{
    public Guid SessionId { get; set; }
    public Guid EpochId { get; set; }
    public int EpochNumber { get; set; }

    public string SessionStatus { get; set; } = string.Empty;
    public string ProtectionMode { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; }
    public DateTime? CompletedUtc { get; set; }

    public string MessageDigestHex { get; set; } = string.Empty;
    public string AggregatePublicKeyHex { get; set; } = string.Empty;
    public string? AggregateNoncePointHex { get; set; }
    public string? ChallengeHex { get; set; }
    public string? AggregateSignatureNoncePointHex { get; set; }
    public string? AggregateSignatureScalarHex { get; set; }

    public bool AllCommitmentsPublished { get; set; }
    public bool AllNoncesRevealed { get; set; }
    public bool AllPartialSignaturesSubmitted { get; set; }

    public List<ProtocolSessionParticipantProjectionEntity> Participants { get; set; } = new();
}