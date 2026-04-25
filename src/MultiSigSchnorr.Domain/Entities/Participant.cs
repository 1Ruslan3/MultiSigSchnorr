using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Domain.Entities;

public sealed class Participant
{
    public Guid Id { get; private set; }
    public string DisplayName { get; private set; }
    public PublicKeyValue PublicKey { get; private set; }
    public ParticipantStatus Status { get; private set; }
    public DateTime CreatedUtc { get; private set; }
    public DateTime? RevokedUtc { get; private set; }

    public Participant(
        Guid id,
        string displayName,
        PublicKeyValue publicKey,
        ParticipantStatus status,
        DateTime createdUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Participant id cannot be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));

        Id = id;
        DisplayName = displayName.Trim();
        PublicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
        Status = status;
        CreatedUtc = createdUtc;
    }

    public void Activate()
    {
        if (Status == ParticipantStatus.Revoked)
            throw new InvalidOperationException("Revoked participant cannot be activated.");

        Status = ParticipantStatus.Active;
    }

    public void Revoke(DateTime revokedUtc)
    {
        if (Status == ParticipantStatus.Revoked)
            return;

        Status = ParticipantStatus.Revoked;
        RevokedUtc = revokedUtc;
    }

}