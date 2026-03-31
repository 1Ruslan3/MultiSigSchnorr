using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Domain.Entities;

public sealed class AggregateSignature
{
    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public PointValue AggregateNoncePoint { get; private set; }
    public SignatureScalarValue SignatureScalar { get; private set; }
    public DateTime CreatedUtc { get; private set; }

    public AggregateSignature(
        Guid id,
        Guid sessionId,
        PointValue aggregateNoncePoint,
        SignatureScalarValue signatureScalar,
        DateTime createdUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Aggregate signature id cannot be empty.", nameof(id));
        if (sessionId == Guid.Empty)
            throw new ArgumentException("Session id cannot be empty.", nameof(sessionId));

        Id = id;
        SessionId = sessionId;
        AggregateNoncePoint = aggregateNoncePoint ?? throw new ArgumentNullException(nameof(aggregateNoncePoint));
        SignatureScalar = signatureScalar ?? throw new ArgumentNullException(nameof(signatureScalar));
        CreatedUtc = createdUtc;
    }

    private AggregateSignature() { }
}