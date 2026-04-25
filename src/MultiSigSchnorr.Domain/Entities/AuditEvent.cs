using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Domain.Entities;

public sealed class AuditEvent
{
    public Guid Id { get; private set; }
    public AuditEventType EventType { get; private set; }
    public string Description { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public DateTime CreatedUtc { get; private set; }

    public AuditEvent(
        Guid id,
        AuditEventType eventType,
        string description,
        Guid? relatedEntityId,
        DateTime createdUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Audit event id cannot be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        Id = id;
        EventType = eventType;
        Description = description.Trim();
        RelatedEntityId = relatedEntityId;
        CreatedUtc = createdUtc;
    }

}