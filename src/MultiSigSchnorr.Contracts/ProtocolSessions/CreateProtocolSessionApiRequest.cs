namespace MultiSigSchnorr.Contracts.ProtocolSessions;

public sealed class CreateProtocolSessionApiRequest
{
    public Guid EpochId { get; init; }
    public IReadOnlyList<Guid> ParticipantIds { get; init; } = Array.Empty<Guid>();
    public string Message { get; init; } = string.Empty;
}