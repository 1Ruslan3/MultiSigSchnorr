namespace MultiSigSchnorr.Domain.Entities;

public sealed class SignatureSession
{
    private readonly List<Guid> _participantIds;

    public Guid Id { get; }
    public Guid EpochId { get; }
    public IReadOnlyList<Guid> ParticipantIds => _participantIds;

    public byte[] Message { get; }
    public DateTime CreatedUtc { get; }

    public bool IsFinalized { get; private set; }

    public SignatureSession(
        Guid id,
        Guid epochId,
        IEnumerable<Guid> participantIds,
        byte[] message,
        DateTime createdUtc)
    {
        if (epochId == Guid.Empty)
            throw new ArgumentException("Epoch id cannot be empty.", nameof(epochId));

        var list = participantIds?.Distinct().ToList()
            ?? throw new ArgumentNullException(nameof(participantIds));

        if (list.Count < 2)
            throw new ArgumentException("At least two participants required.");

        if (message == null || message.Length == 0)
            throw new ArgumentException("Message cannot be empty.", nameof(message));

        Id = id;
        EpochId = epochId;
        _participantIds = list;
        Message = message;
        CreatedUtc = createdUtc;
    }

    public void FinalizeSession()
    {
        if (IsFinalized)
            throw new InvalidOperationException("Session already finalized.");

        IsFinalized = true;
    }
}