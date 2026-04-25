namespace MultiSigSchnorr.Infrastructure.Persistence.Entities;

public sealed class ProtocolSessionParticipantProjectionEntity
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }
    public Guid ParticipantId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public bool HasCommitment { get; set; }
    public bool HasReveal { get; set; }
    public bool HasPartialSignature { get; set; }

    public string PublicKeyHex { get; set; } = string.Empty;
    public string AggregationCoefficientHex { get; set; } = string.Empty;

    public string? CommitmentHex { get; set; }
    public string? PublicNoncePointHex { get; set; }
    public string? PartialSignatureHex { get; set; }

    public ProtocolSessionProjectionEntity? Session { get; set; }
}