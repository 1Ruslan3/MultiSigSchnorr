namespace MultiSigSchnorr.Infrastructure.Persistence.Entities;

public sealed class AuditLogEntryEntity
{
    public Guid Id { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string MetadataJson { get; set; } = "{}";
    public DateTime CreatedUtc { get; set; }
}