using System.Collections.Concurrent;
using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Entities;

namespace MultiSigSchnorr.Infrastructure.Repositories;

public sealed class InMemoryAuditLogRepository : IAuditLogRepository
{
    private readonly ConcurrentDictionary<Guid, AuditLogEntry> _entries = new();

    public Task AddAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (!_entries.TryAdd(entry.Id, entry))
            throw new InvalidOperationException($"Audit log entry '{entry.Id}' already exists.");

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AuditLogEntry>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AuditLogEntry> result = _entries.Values
            .OrderByDescending(x => x.CreatedUtc)
            .ToList();

        return Task.FromResult(result);
    }
}