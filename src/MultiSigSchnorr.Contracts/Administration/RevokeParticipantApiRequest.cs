namespace MultiSigSchnorr.Contracts.Administration;

public sealed class RevokeParticipantApiRequest
{
    public string Reason { get; init; } = string.Empty;
}