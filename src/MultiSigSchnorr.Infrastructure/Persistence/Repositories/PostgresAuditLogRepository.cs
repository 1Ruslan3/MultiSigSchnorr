using Microsoft.EntityFrameworkCore;
using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Infrastructure.Persistence.Entities;

namespace MultiSigSchnorr.Infrastructure.Persistence.Repositories;

public sealed class PostgresAuditLogRepository : IAuditLogRepository
{
    private readonly MultiSigSchnorrDbContext _dbContext;

    public PostgresAuditLogRepository(MultiSigSchnorrDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(
        AuditLogEntry entry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var entity = new AuditLogEntryEntity
        {
            Id = entry.Id,
            ActionType = entry.ActionType.ToString(),
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            Description = entry.Description,
            MetadataJson = string.IsNullOrWhiteSpace(entry.MetadataJson)
                ? "{}"
                : entry.MetadataJson,
            CreatedUtc = entry.CreatedUtc
        };

        await _dbContext.AuditLogEntries.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLogEntry>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.AuditLogEntries
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedUtc)
            .ToListAsync(cancellationToken);

        return entities
            .Select(MapToDomain)
            .ToList();
    }

    private static AuditLogEntry MapToDomain(AuditLogEntryEntity entity)
    {
        if (!Enum.TryParse<AuditActionType>(
                entity.ActionType,
                ignoreCase: true,
                out var actionType))
        {
            throw new InvalidOperationException(
                $"Unsupported audit action type '{entity.ActionType}'.");
        }

        return new AuditLogEntry(
            entity.Id,
            actionType,
            entity.EntityType,
            entity.EntityId,
            entity.Description,
            entity.MetadataJson,
            entity.CreatedUtc);
    }
}