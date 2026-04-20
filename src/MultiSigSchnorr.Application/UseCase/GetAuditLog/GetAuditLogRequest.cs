namespace MultiSigSchnorr.Application.UseCases.GetAuditLog;

public sealed class GetAuditLogRequest
{
    public int Take { get; init; } = 100;
}