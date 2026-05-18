namespace MultiSigSchnorr.Contracts.Administration;

public sealed class RenameParticipantApiRequest
{
    public string DisplayName { get; init; } = string.Empty;
}