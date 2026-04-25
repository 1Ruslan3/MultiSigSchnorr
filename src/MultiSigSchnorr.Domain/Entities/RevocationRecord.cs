namespace MultiSigSchnorr.Domain.Entities;

public sealed class RevocationRecord
{
    public Guid Id { get; private set; }
    public Guid ParticipantId { get; private set; }
    public Guid EpochId { get; private set; }
    public string Reason { get; private set; }
    public DateTime RevokedUtc { get; private set; }

    public RevocationRecord(Guid id, Guid participantId, Guid epochId, string reason, DateTime revokedUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Revocation id cannot be empty.", nameof(id));
        if (participantId == Guid.Empty)
            throw new ArgumentException("Participant id cannot be empty.", nameof(participantId));
        if (epochId == Guid.Empty)
            throw new ArgumentException("Epoch id cannot be empty.", nameof(epochId));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason cannot be empty.", nameof(reason));

        Id = id;
        ParticipantId = participantId;
        EpochId = epochId;
        Reason = reason.Trim();
        RevokedUtc = revokedUtc;
    }

}