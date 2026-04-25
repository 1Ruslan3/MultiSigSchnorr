using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Domain.Entities;

public sealed class NonceReveal
{
    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public Guid ParticipantId { get; private set; }
    public PointValue PublicNoncePoint { get; private set; }
    public DateTime SubmittedUtc { get; private set; }

    public NonceReveal(
        Guid id,
        Guid sessionId,
        Guid participantId,
        PointValue publicNoncePoint,
        DateTime submittedUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Nonce reveal id cannot be empty.", nameof(id));
        if (sessionId == Guid.Empty)
            throw new ArgumentException("Session id cannot be empty.", nameof(sessionId));
        if (participantId == Guid.Empty)
            throw new ArgumentException("Participant id cannot be empty.", nameof(participantId));

        Id = id;
        SessionId = sessionId;
        ParticipantId = participantId;
        PublicNoncePoint = publicNoncePoint ?? throw new ArgumentNullException(nameof(publicNoncePoint));
        SubmittedUtc = submittedUtc;
    }

}