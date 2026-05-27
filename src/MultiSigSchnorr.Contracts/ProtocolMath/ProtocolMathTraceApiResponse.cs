namespace MultiSigSchnorr.Contracts.ProtocolMath;

public sealed class ProtocolMathTraceApiResponse
{
    public Guid SessionId { get; init; }
    public Guid EpochId { get; init; }
    public int EpochNumber { get; init; }
    public string SessionStatus { get; init; } = string.Empty;
    public string ProtectionMode { get; init; } = string.Empty;

    public string MessageDigestHex { get; init; } = string.Empty;
    public string AggregatePublicKeyHex { get; init; } = string.Empty;
    public string AggregateNoncePointHex { get; init; } = string.Empty;
    public string ChallengeHex { get; init; } = string.Empty;
    public string AggregateSignatureScalarHex { get; init; } = string.Empty;

    public IReadOnlyList<ProtocolMathParticipantTraceApiResponse> Participants { get; init; }
        = Array.Empty<ProtocolMathParticipantTraceApiResponse>();

    public ProtocolMathFinalVerificationApiResponse FinalVerification { get; init; }
        = new();

    public IReadOnlyList<ProtocolMathStepApiResponse> Steps { get; init; }
        = Array.Empty<ProtocolMathStepApiResponse>();
}

public sealed class ProtocolMathParticipantTraceApiResponse
{
    public Guid ParticipantId { get; init; }
    public string DisplayName { get; init; } = string.Empty;

    public string PublicKeyHex { get; init; } = string.Empty;
    public string AggregationCoefficientHex { get; init; } = string.Empty;
    public string CommitmentHex { get; init; } = string.Empty;
    public string PublicNoncePointHex { get; init; } = string.Empty;
    public string PartialSignatureHex { get; init; } = string.Empty;

    public string CommitmentRecomputedHex { get; init; } = string.Empty;
    public bool? CommitmentMatchesPublicNonce { get; init; }

    public string PartialSignatureLeftPointHex { get; init; } = string.Empty;
    public string PartialSignatureRightPointHex { get; init; } = string.Empty;
    public bool? PartialSignatureEquationHolds { get; init; }
}

public sealed class ProtocolMathFinalVerificationApiResponse
{
    public string LeftPointHex { get; init; } = string.Empty;
    public string RightPointHex { get; init; } = string.Empty;
    public bool? EquationHolds { get; init; }
}

public sealed class ProtocolMathStepApiResponse
{
    public int Number { get; init; }
    public string Stage { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Formula { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsComplete { get; init; }

    public IReadOnlyDictionary<string, string> Values { get; init; }
        = new Dictionary<string, string>();
}
