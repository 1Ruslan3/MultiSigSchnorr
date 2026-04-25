using Microsoft.EntityFrameworkCore;
using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Infrastructure.Persistence.Entities;

namespace MultiSigSchnorr.Infrastructure.Persistence.Repositories;

public sealed class PostgresEpochRepository : IEpochRepository
{
    private readonly MultiSigSchnorrDbContext _dbContext;

    public PostgresEpochRepository(MultiSigSchnorrDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(Epoch epoch, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(epoch);

        var exists = await _dbContext.Epochs
            .AnyAsync(x => x.Id == epoch.Id, cancellationToken);

        if (exists)
            throw new InvalidOperationException($"Epoch '{epoch.Id}' already exists.");

        await _dbContext.Epochs.AddAsync(MapToEntity(epoch), cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Epoch?> GetByIdAsync(Guid epochId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Epochs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == epochId, cancellationToken);

        return entity is null ? null : MapToDomain(entity);
    }

    public async Task<IReadOnlyList<Epoch>> ListAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.Epochs
            .AsNoTracking()
            .OrderByDescending(x => x.Number)
            .ToListAsync(cancellationToken);

        return entities
            .Select(MapToDomain)
            .ToList();
    }

    public async Task UpdateAsync(Epoch epoch, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(epoch);

        var entity = await _dbContext.Epochs
            .FirstOrDefaultAsync(x => x.Id == epoch.Id, cancellationToken);

        if (entity is null)
            throw new InvalidOperationException($"Epoch '{epoch.Id}' does not exist.");

        entity.Number = epoch.Number;
        entity.Status = epoch.Status.ToString();
        entity.CreatedUtc = epoch.CreatedUtc;
        entity.ActivatedUtc = epoch.ActivatedUtc;
        entity.ClosedUtc = epoch.ClosedUtc;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static EpochEntity MapToEntity(Epoch epoch)
    {
        return new EpochEntity
        {
            Id = epoch.Id,
            Number = epoch.Number,
            Status = epoch.Status.ToString(),
            CreatedUtc = epoch.CreatedUtc,
            ActivatedUtc = epoch.ActivatedUtc,
            ClosedUtc = epoch.ClosedUtc
        };
    }

    private static Epoch MapToDomain(EpochEntity entity)
    {
        if (!Enum.TryParse<EpochStatus>(entity.Status, ignoreCase: true, out var status))
        {
            throw new InvalidOperationException(
                $"Unsupported epoch status '{entity.Status}'.");
        }

        var epoch = new Epoch(entity.Id, entity.Number, entity.CreatedUtc);

        switch (status)
        {
            case EpochStatus.Draft:
                return epoch;

            case EpochStatus.Active:
                epoch.Activate(entity.ActivatedUtc ?? entity.CreatedUtc);
                return epoch;

            case EpochStatus.Closed:
                if (entity.ActivatedUtc.HasValue)
                    epoch.Activate(entity.ActivatedUtc.Value);

                epoch.Close(entity.ClosedUtc ?? entity.CreatedUtc);
                return epoch;

            case EpochStatus.Archived:
                throw new InvalidOperationException(
                    "Archived epochs are not supported by the current domain model.");

            default:
                throw new InvalidOperationException(
                    $"Unsupported epoch status '{entity.Status}'.");
        }
    }
}