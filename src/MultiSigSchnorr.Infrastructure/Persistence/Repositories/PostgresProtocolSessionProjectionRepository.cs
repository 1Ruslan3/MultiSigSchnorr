using Microsoft.EntityFrameworkCore;
using MultiSigSchnorr.Application.Projections;
using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Infrastructure.Persistence.Entities;
using MultiSigSchnorr.Protocol.Models;

namespace MultiSigSchnorr.Infrastructure.Persistence.Repositories;

public sealed class PostgresProtocolSessionProjectionRepository : IProtocolSessionProjectionRepository
{
    private readonly MultiSigSchnorrDbContext _dbContext;

    public PostgresProtocolSessionProjectionRepository(MultiSigSchnorrDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task UpsertAsync(
        NPartyProtocolSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        var entity = await _dbContext.ProtocolSessions
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.SessionId == session.SessionId, cancellationToken);

        if (entity is null)
        {
            entity = new ProtocolSessionProjectionEntity
            {
                SessionId = session.SessionId
            };

            await _dbContext.ProtocolSessions.AddAsync(entity, cancellationToken);
        }

        FillSessionEntity(entity, session);
        UpsertParticipantEntities(entity, session);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProtocolSessionProjection?> GetByIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ProtocolSessions
            .AsNoTracking()
            .Include(x => x.Participants)
            .FirstOrDefaultAsync(x => x.SessionId == sessionId, cancellationToken);

        return entity is null ? null : MapToProjection(entity);
    }

    public async Task<IReadOnlyList<ProtocolSessionProjection>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.ProtocolSessions
            .AsNoTracking()
            .Include(x => x.Participants)
            .OrderByDescending(x => x.CreatedUtc)
            .ToListAsync(cancellationToken);

        return entities
            .Select(MapToProjection)
            .ToList();
    }

    private void UpsertParticipantEntities(
        ProtocolSessionProjectionEntity entity,
        NPartyProtocolSession session)
    {
        var runtimeParticipants = session.Participants.Values
            .OrderBy(x => x.DisplayName, StringComparer.Ordinal)
            .ToList();

        var runtimeParticipantIds = runtimeParticipants
            .Select(x => x.ParticipantId)
            .ToHashSet();

        var obsoleteEntities = entity.Participants
            .Where(x => !runtimeParticipantIds.Contains(x.ParticipantId))
            .ToList();

        if (obsoleteEntities.Count > 0)
            _dbContext.ProtocolSessionParticipants.RemoveRange(obsoleteEntities);

        var existingByParticipantId = entity.Participants
            .Where(x => runtimeParticipantIds.Contains(x.ParticipantId))
            .ToDictionary(x => x.ParticipantId);

        foreach (var participant in runtimeParticipants)
        {
            if (!existingByParticipantId.TryGetValue(participant.ParticipantId, out var participantEntity))
            {
                participantEntity = new ProtocolSessionParticipantProjectionEntity
                {
                    Id = Guid.NewGuid(),
                    SessionId = session.SessionId,
                    ParticipantId = participant.ParticipantId
                };

                entity.Participants.Add(participantEntity);
            }

            FillParticipantEntity(participantEntity, participant);
        }
    }

    private static void FillSessionEntity(
        ProtocolSessionProjectionEntity entity,
        NPartyProtocolSession session)
    {
        entity.EpochId = session.Epoch.Id;
        entity.EpochNumber = session.Epoch.Number;
        entity.SessionStatus = session.SigningSession.Status.ToString();
        entity.ProtectionMode = session.ProtectionMode.ToString();
        entity.CreatedUtc = session.SigningSession.CreatedUtc;
        entity.CompletedUtc = session.SigningSession.CompletedUtc;
        entity.MessageDigestHex = session.MessageDigest.ToHex();
        entity.AggregatePublicKeyHex = session.AggregatePublicKey.ToHex();
        entity.AggregateNoncePointHex = session.AggregateNoncePoint?.ToHex();
        entity.ChallengeHex = session.Challenge?.ToHex();
        entity.AggregateSignatureNoncePointHex = session.AggregateSignature?.AggregateNoncePoint.ToHex();
        entity.AggregateSignatureScalarHex = session.AggregateSignature?.SignatureScalar.ToHex();
        entity.AllCommitmentsPublished = session.AllCommitmentsPublished;
        entity.AllNoncesRevealed = session.AllNoncesRevealed;
        entity.AllPartialSignaturesSubmitted = session.AllPartialSignaturesSubmitted;
    }

    private static void FillParticipantEntity(
        ProtocolSessionParticipantProjectionEntity entity,
        NPartyParticipantProtocolState participant)
    {
        entity.DisplayName = participant.DisplayName;
        entity.HasCommitment = participant.HasCommitment;
        entity.HasReveal = participant.HasReveal;
        entity.HasPartialSignature = participant.HasPartialSignature;
        entity.PublicKeyHex = participant.PublicKey.ToHex();
        entity.AggregationCoefficientHex = participant.AggregationCoefficient.ToHex();
        entity.CommitmentHex = participant.CommitmentRecord?.Commitment.ToHex();
        entity.PublicNoncePointHex = participant.RevealRecord?.PublicNoncePoint.ToHex();
        entity.PartialSignatureHex = participant.PartialSignatureRecord?.SignatureScalar.ToHex();
    }

    private static ProtocolSessionProjection MapToProjection(
        ProtocolSessionProjectionEntity entity)
    {
        if (!Enum.TryParse<SessionStatus>(
                entity.SessionStatus,
                ignoreCase: true,
                out var sessionStatus))
        {
            throw new InvalidOperationException(
                $"Unsupported session status '{entity.SessionStatus}'.");
        }

        if (!Enum.TryParse<SignatureProtectionMode>(
                entity.ProtectionMode,
                ignoreCase: true,
                out var protectionMode))
        {
            throw new InvalidOperationException(
                $"Unsupported protection mode '{entity.ProtectionMode}'.");
        }

        return new ProtocolSessionProjection
        {
            SessionId = entity.SessionId,
            EpochId = entity.EpochId,
            EpochNumber = entity.EpochNumber,
            SessionStatus = sessionStatus,
            ProtectionMode = protectionMode,
            CreatedUtc = entity.CreatedUtc,
            CompletedUtc = entity.CompletedUtc,
            MessageDigestHex = entity.MessageDigestHex,
            AggregatePublicKeyHex = entity.AggregatePublicKeyHex,
            AggregateNoncePointHex = entity.AggregateNoncePointHex,
            ChallengeHex = entity.ChallengeHex,
            AggregateSignatureNoncePointHex = entity.AggregateSignatureNoncePointHex,
            AggregateSignatureScalarHex = entity.AggregateSignatureScalarHex,
            AllCommitmentsPublished = entity.AllCommitmentsPublished,
            AllNoncesRevealed = entity.AllNoncesRevealed,
            AllPartialSignaturesSubmitted = entity.AllPartialSignaturesSubmitted,
            Participants = entity.Participants
                .OrderBy(x => x.DisplayName, StringComparer.Ordinal)
                .Select(x => new ProtocolSessionParticipantProjection
                {
                    ParticipantId = x.ParticipantId,
                    DisplayName = x.DisplayName,
                    HasCommitment = x.HasCommitment,
                    HasReveal = x.HasReveal,
                    HasPartialSignature = x.HasPartialSignature,
                    PublicKeyHex = x.PublicKeyHex,
                    AggregationCoefficientHex = x.AggregationCoefficientHex,
                    CommitmentHex = x.CommitmentHex,
                    PublicNoncePointHex = x.PublicNoncePointHex,
                    PartialSignatureHex = x.PartialSignatureHex
                })
                .ToList()
        };
    }
}