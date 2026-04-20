using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Application.UseCases.GetAuditLog;

public sealed class GetAuditLogRequest
{
    public int Take { get; init; } = 100;

    public string? SearchTerm { get; init; }

    public AuditActionType? ActionType { get; init; }

    public string? EntityType { get; init; }

    public Guid? EntityId { get; init; }
}