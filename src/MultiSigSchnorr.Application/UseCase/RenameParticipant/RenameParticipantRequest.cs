namespace MultiSigSchnorr.Application.UseCases.RenameParticipant;

public sealed class RenameParticipantRequest
{
    public Guid ParticipantId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
}