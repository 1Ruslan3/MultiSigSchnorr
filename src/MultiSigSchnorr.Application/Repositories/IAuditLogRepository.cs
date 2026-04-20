using MultiSigSchnorr.Domain.Entities;

namespace MultiSigSchnorr.Application.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLogEntry>> ListAsync(CancellationToken cancellationToken = default);
}