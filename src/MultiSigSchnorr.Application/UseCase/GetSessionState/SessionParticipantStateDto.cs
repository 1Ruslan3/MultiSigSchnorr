namespace MultiSigSchnorr.Application.UseCases.GetSessionState;

public sealed class SessionParticipantStateDto
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