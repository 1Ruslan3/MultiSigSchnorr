namespace MultiSigSchnorr.Contracts.Administration;

public sealed class CreateParticipantApiRequest
{
    public string DisplayName { get; init; } = string.Empty;
}