using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Application.UseCases.GetAuditLog;

public sealed class AuditLogItemDto
{
    public Guid Id { get; init; }
    public AuditActionType ActionType { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public Guid? EntityId { get; init; }
    public string Description { get; init; } = string.Empty;
    public string MetadataJson { get; init; } = string.Empty;
    public DateTime CreatedUtc { get; init; }
}