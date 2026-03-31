using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Domain.Entities;

public sealed class NonceCommitment
{
    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public Guid ParticipantId { get; private set; }
    public CommitmentValue Commitment { get; private set; }
    public DateTime SubmittedUtc { get; private set; }

    public NonceCommitment(
        Guid id,
        Guid sessionId,
        Guid participantId,
        CommitmentValue commitment,
        DateTime submittedUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Commitment id cannot be empty.", nameof(id));
        if (sessionId == Guid.Empty)
            throw new ArgumentException("Session id cannot be empty.", nameof(sessionId));
        if (participantId == Guid.Empty)
            throw new ArgumentException("Participant id cannot be empty.", nameof(participantId));

        Id = id;
        SessionId = sessionId;
        ParticipantId = participantId;
        Commitment = commitment ?? throw new ArgumentNullException(nameof(commitment));
        SubmittedUtc = submittedUtc;
    }

    private NonceCommitment() { }
}