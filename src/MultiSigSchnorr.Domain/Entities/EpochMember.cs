namespace MultiSigSchnorr.Domain.Entities;

public sealed class EpochMember
{
    public Guid Id { get; private set; }
    public Guid EpochId { get; private set; }
    public Guid ParticipantId { get; private set; }
    public DateTime AddedUtc { get; private set; }
    public bool IsActive { get; private set; }

    public EpochMember(Guid id, Guid epochId, Guid participantId, DateTime addedUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Epoch member id cannot be empty.", nameof(id));
        if (epochId == Guid.Empty)
            throw new ArgumentException("Epoch id cannot be empty.", nameof(epochId));
        if (participantId == Guid.Empty)
            throw new ArgumentException("Participant id cannot be empty.", nameof(participantId));

        Id = id;
        EpochId = epochId;
        ParticipantId = participantId;
        AddedUtc = addedUtc;
        IsActive = true;
    }

    public void Deactivate() => IsActive = false;

    private EpochMember() { }
}