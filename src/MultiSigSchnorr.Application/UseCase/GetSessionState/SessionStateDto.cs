using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Application.UseCases.GetSessionState;

public sealed class SessionStateDto
{
    public Guid SessionId { get; init; }
    public Guid EpochId { get; init; }
    public int EpochNumber { get; init; }

    public SessionStatus SessionStatus { get; init; }

    public string MessageDigestHex { get; init; } = string.Empty;
    public string AggregatePublicKeyHex { get; init; } = string.Empty;
    public string? AggregateNoncePointHex { get; init; }
    public string? ChallengeHex { get; init; }
    public string? AggregateSignatureNoncePointHex { get; init; }
    public string? AggregateSignatureScalarHex { get; init; }

    public bool AllCommitmentsPublished { get; init; }
    public bool AllNoncesRevealed { get; init; }
    public bool AllPartialSignaturesSubmitted { get; init; }

    public IReadOnlyList<SessionParticipantStateDto> Participants { get; init; }
        = Array.Empty<SessionParticipantStateDto>();
}