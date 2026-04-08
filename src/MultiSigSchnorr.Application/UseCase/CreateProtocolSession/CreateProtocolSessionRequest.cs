namespace MultiSigSchnorr.Application.UseCases.CreateProtocolSession;

public sealed class CreateProtocolSessionRequest
{
    public Guid EpochId { get; init; }
    public IReadOnlyList<Guid> ParticipantIds { get; init; } = Array.Empty<Guid>();
    public string Message { get; init; } = string.Empty;
}