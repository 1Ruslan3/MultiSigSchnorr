using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Domain.Entities;

public sealed class AuditLogEntry
{
    public Guid Id { get; }
    public AuditActionType ActionType { get; }
    public string EntityType { get; }
    public Guid? EntityId { get; }
    public string Description { get; }
    public string MetadataJson { get; }
    public DateTime CreatedUtc { get; }

    public AuditLogEntry(
        Guid id,
        AuditActionType actionType,
        string entityType,
        Guid? entityId,
        string description,
        string metadataJson,
        DateTime createdUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Audit log id cannot be empty.", nameof(id));

        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Id = id;
        ActionType = actionType;
        EntityType = entityType;
        EntityId = entityId;
        Description = description;
        MetadataJson = metadataJson ?? string.Empty;
        CreatedUtc = createdUtc;
    }
}