using Microsoft.EntityFrameworkCore;
using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Infrastructure.Persistence.Entities;

namespace MultiSigSchnorr.Infrastructure.Persistence.Repositories;

public sealed class PostgresEpochMemberRepository : IEpochMemberRepository
{
    private readonly MultiSigSchnorrDbContext _dbContext;

    public PostgresEpochMemberRepository(MultiSigSchnorrDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(EpochMember member, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(member);

        var exists = await _dbContext.EpochMembers
            .AnyAsync(x => x.Id == member.Id, cancellationToken);

        if (exists)
            throw new InvalidOperationException($"Epoch member '{member.Id}' already exists.");

        await _dbContext.EpochMembers.AddAsync(MapToEntity(member), cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EpochMember>> GetByEpochIdAsync(
        Guid epochId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.EpochMembers
            .AsNoTracking()
            .Where(x => x.EpochId == epochId)
            .OrderBy(x => x.AddedUtc)
            .ToListAsync(cancellationToken);

        return entities
            .Select(MapToDomain)
            .ToList();
    }

    public async Task<IReadOnlyList<EpochMember>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.EpochMembers
            .AsNoTracking()
            .OrderBy(x => x.AddedUtc)
            .ToListAsync(cancellationToken);

        return entities
            .Select(MapToDomain)
            .ToList();
    }

    public async Task UpdateAsync(EpochMember member, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(member);

        var entity = await _dbContext.EpochMembers
            .FirstOrDefaultAsync(x => x.Id == member.Id, cancellationToken);

        if (entity is null)
            throw new InvalidOperationException($"Epoch member '{member.Id}' does not exist.");

        entity.EpochId = member.EpochId;
        entity.ParticipantId = member.ParticipantId;
        entity.AddedUtc = member.AddedUtc;
        entity.IsActive = member.IsActive;

        if (!member.IsActive && entity.RemovedUtc is null)
            entity.RemovedUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static EpochMemberEntity MapToEntity(EpochMember member)
    {
        return new EpochMemberEntity
        {
            Id = member.Id,
            EpochId = member.EpochId,
            ParticipantId = member.ParticipantId,
            AddedUtc = member.AddedUtc,
            RemovedUtc = member.IsActive ? null : DateTime.UtcNow,
            IsActive = member.IsActive
        };
    }

    private static EpochMember MapToDomain(EpochMemberEntity entity)
    {
        var member = new EpochMember(
            entity.Id,
            entity.EpochId,
            entity.ParticipantId,
            entity.AddedUtc);

        if (!entity.IsActive)
            member.Deactivate();

        return member;
    }
}