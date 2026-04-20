using System.Text.Json;
using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Application.Audit;

public sealed class AuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository
            ?? throw new ArgumentNullException(nameof(auditLogRepository));
    }

    public Task LogProtocolSessionCreatedAsync(
        Guid sessionId,
        Guid epochId,
        int epochNumber,
        SignatureProtectionMode protectionMode,
        IReadOnlyCollection<Guid> participantIds,
        DateTime createdUtc,
        CancellationToken cancellationToken = default)
    {
        var metadataJson = JsonSerializer.Serialize(new
        {
            sessionId,
            epochId,
            epochNumber,
            protectionMode,
            participantIds
        });

        var entry = new AuditLogEntry(
            Guid.NewGuid(),
            AuditActionType.ProtocolSessionCreated,
            "ProtocolSession",
            sessionId,
            $"Protocol session '{sessionId}' was created using mode '{protectionMode}'.",
            metadataJson,
            createdUtc);

        return _auditLogRepository.AddAsync(entry, cancellationToken);
    }

    public Task LogParticipantRevokedAsync(
        Guid participantId,
        Guid epochId,
        string reason,
        DateTime createdUtc,
        CancellationToken cancellationToken = default)
    {
        var metadataJson = JsonSerializer.Serialize(new
        {
            participantId,
            epochId,
            reason
        });

        var entry = new AuditLogEntry(
            Guid.NewGuid(),
            AuditActionType.ParticipantRevoked,
            "Participant",
            participantId,
            $"Participant '{participantId}' was revoked from active epoch '{epochId}'.",
            metadataJson,
            createdUtc);

        return _auditLogRepository.AddAsync(entry, cancellationToken);
    }

    public Task LogEpochTransitionedAsync(
        Guid previousEpochId,
        Guid newEpochId,
        int newEpochNumber,
        int participantsCarried,
        DateTime createdUtc,
        CancellationToken cancellationToken = default)
    {
        var metadataJson = JsonSerializer.Serialize(new
        {
            previousEpochId,
            newEpochId,
            newEpochNumber,
            participantsCarried
        });

        var entry = new AuditLogEntry(
            Guid.NewGuid(),
            AuditActionType.EpochTransitioned,
            "Epoch",
            newEpochId,
            $"Epoch transitioned from '{previousEpochId}' to '{newEpochId}' (number {newEpochNumber}).",
            metadataJson,
            createdUtc);

        return _auditLogRepository.AddAsync(entry, cancellationToken);
    }
}