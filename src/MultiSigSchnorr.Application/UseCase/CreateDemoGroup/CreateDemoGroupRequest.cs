namespace MultiSigSchnorr.Application.UseCases.CreateDemoGroup;

public sealed class CreateDemoGroupRequest
{
    public int ParticipantsCount { get; init; } = 3;
    public string DisplayNamePrefix { get; init; } = "Demo Signer";
}
