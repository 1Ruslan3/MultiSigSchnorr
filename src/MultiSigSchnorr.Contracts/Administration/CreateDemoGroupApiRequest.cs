namespace MultiSigSchnorr.Contracts.Administration;

public sealed class CreateDemoGroupApiRequest
{
    public int ParticipantsCount { get; init; } = 3;
    public string DisplayNamePrefix { get; init; } = "Demo Signer";
}
