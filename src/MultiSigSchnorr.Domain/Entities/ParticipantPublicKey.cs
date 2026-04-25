using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Domain.Entities;

public sealed class ParticipantPublicKey
{
    public Guid Id { get; private set; }
    public Guid ParticipantId { get; private set; }
    public string CurveName { get; private set; }
    public PublicKeyValue PublicKey { get; private set; }
    public DateTime CreatedUtc { get; private set; }

    public ParticipantPublicKey(
        Guid id,
        Guid participantId,
        string curveName,
        PublicKeyValue publicKey,
        DateTime createdUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Key id cannot be empty.", nameof(id));
        if (participantId == Guid.Empty)
            throw new ArgumentException("Participant id cannot be empty.", nameof(participantId));
        if (string.IsNullOrWhiteSpace(curveName))
            throw new ArgumentException("Curve name cannot be empty.", nameof(curveName));

        Id = id;
        ParticipantId = participantId;
        CurveName = curveName.Trim();
        PublicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
        CreatedUtc = createdUtc;
    }

}