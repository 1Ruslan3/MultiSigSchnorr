using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Domain.Entities;

public sealed class PartialSignature
{
    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public Guid ParticipantId { get; private set; }
    public SignatureScalarValue SignatureScalar { get; private set; }
    public DateTime SubmittedUtc { get; private set; }

    public PartialSignature(
        Guid id,
        Guid sessionId,
        Guid participantId,
        SignatureScalarValue signatureScalar,
        DateTime submittedUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Partial signature id cannot be empty.", nameof(id));
        if (sessionId == Guid.Empty)
            throw new ArgumentException("Session id cannot be empty.", nameof(sessionId));
        if (participantId == Guid.Empty)
            throw new ArgumentException("Participant id cannot be empty.", nameof(participantId));

        Id = id;
        SessionId = sessionId;
        ParticipantId = participantId;
        SignatureScalar = signatureScalar ?? throw new ArgumentNullException(nameof(signatureScalar));
        SubmittedUtc = submittedUtc;
    }

    private PartialSignature() { }
}