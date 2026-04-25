using Microsoft.EntityFrameworkCore;
using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Domain.ValueObjects;
using MultiSigSchnorr.Infrastructure.Persistence.Entities;

namespace MultiSigSchnorr.Infrastructure.Persistence.Repositories;

public sealed class PostgresParticipantRepository : IParticipantRepository
{
    private readonly MultiSigSchnorrDbContext _dbContext;

    public PostgresParticipantRepository(MultiSigSchnorrDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(
        Participant participant,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(participant);

        var exists = await _dbContext.Participants
            .AnyAsync(x => x.Id == participant.Id, cancellationToken);

        if (exists)
            throw new InvalidOperationException($"Participant '{participant.Id}' already exists.");

        await _dbContext.Participants.AddAsync(
            MapToEntity(participant),
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Participant?> GetByIdAsync(
        Guid participantId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Participants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == participantId, cancellationToken);

        return entity is null ? null : MapToDomain(entity);
    }

    public async Task<IReadOnlyList<Participant>> GetByIdsAsync(
        IReadOnlyCollection<Guid> participantIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(participantIds);

        var ids = participantIds
            .Distinct()
            .ToList();

        var entities = await _dbContext.Participants
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var map = entities.ToDictionary(x => x.Id);

        return ids
            .Where(map.ContainsKey)
            .Select(id => MapToDomain(map[id]))
            .ToList();
    }

    public async Task<IReadOnlyList<Participant>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.Participants
            .AsNoTracking()
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);

        return entities
            .Select(MapToDomain)
            .ToList();
    }

    public async Task UpdateAsync(
        Participant participant,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(participant);

        var entity = await _dbContext.Participants
            .FirstOrDefaultAsync(x => x.Id == participant.Id, cancellationToken);

        if (entity is null)
            throw new InvalidOperationException($"Participant '{participant.Id}' does not exist.");

        entity.DisplayName = participant.DisplayName;
        entity.PublicKeyHex = participant.PublicKey.ToHex();
        entity.Status = participant.Status.ToString();
        entity.CreatedUtc = participant.CreatedUtc;
        entity.RevokedUtc = participant.RevokedUtc;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static ParticipantEntity MapToEntity(Participant participant)
    {
        return new ParticipantEntity
        {
            Id = participant.Id,
            DisplayName = participant.DisplayName,
            PublicKeyHex = participant.PublicKey.ToHex(),
            Status = participant.Status.ToString(),
            CreatedUtc = participant.CreatedUtc,
            RevokedUtc = participant.RevokedUtc
        };
    }

    private static Participant MapToDomain(ParticipantEntity entity)
    {
        if (!Enum.TryParse<ParticipantStatus>(
                entity.Status,
                ignoreCase: true,
                out var status))
        {
            throw new InvalidOperationException(
                $"Unsupported participant status '{entity.Status}'.");
        }

        var publicKey = PublicKeyValue.FromHex(entity.PublicKeyHex);

        if (status == ParticipantStatus.Revoked)
        {
            return CreateRevokedParticipant(entity, publicKey);
        }

        return new Participant(
            entity.Id,
            entity.DisplayName,
            publicKey,
            status,
            entity.CreatedUtc);
    }

    private static Participant CreateRevokedParticipant(
        ParticipantEntity entity,
        PublicKeyValue publicKey)
    {
        var participant = new Participant(
            entity.Id,
            entity.DisplayName,
            publicKey,
            ParticipantStatus.Active,
            entity.CreatedUtc);

        participant.Revoke(entity.RevokedUtc ?? entity.CreatedUtc);

        return participant;
    }
}