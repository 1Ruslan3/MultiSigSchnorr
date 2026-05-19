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

    public Task LogParticipantCreatedAsync(
        Guid participantId,
        string displayName,
        string publicKeyHex,
        DateTime createdUtc,
        CancellationToken cancellationToken = default)
    {
        var metadataJson = JsonSerializer.Serialize(new
        {
            participantId,
            displayName,
            publicKeyHex
        });

        var entry = new AuditLogEntry(
            Guid.NewGuid(),
            AuditActionType.ParticipantCreated,
            "Participant",
            participantId,
            $"Participant '{displayName}' ({participantId}) was created.",
            metadataJson,
            createdUtc);

        return _auditLogRepository.AddAsync(entry, cancellationToken);
    }

    public Task LogParticipantRenamedAsync(
        Guid participantId,
        string oldDisplayName,
        string newDisplayName,
        DateTime createdUtc,
        CancellationToken cancellationToken = default)
    {
        var metadataJson = JsonSerializer.Serialize(new
        {
            participantId,
            oldDisplayName,
            newDisplayName
        });

        var entry = new AuditLogEntry(
            Guid.NewGuid(),
            AuditActionType.ParticipantRenamed,
            "Participant",
            participantId,
            $"Participant '{participantId}' was renamed from '{oldDisplayName}' to '{newDisplayName}'.",
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

    public Task LogEpochCreatedWithMembersAsync(
        Guid newEpochId,
        int newEpochNumber,
        IReadOnlyCollection<Guid> closedEpochIds,
        IReadOnlyCollection<Guid> participantIds,
        DateTime createdUtc,
        CancellationToken cancellationToken = default)
    {
        var metadataJson = JsonSerializer.Serialize(new
        {
            newEpochId,
            newEpochNumber,
            closedEpochIds,
            participantIds,
            participantsCount = participantIds.Count
        });

        var entry = new AuditLogEntry(
            Guid.NewGuid(),
            AuditActionType.EpochCreatedWithMembers,
            "Epoch",
            newEpochId,
            $"Epoch '{newEpochId}' (number {newEpochNumber}) was created with {participantIds.Count} selected participants.",
            metadataJson,
            createdUtc);

        return _auditLogRepository.AddAsync(entry, cancellationToken);
    }

    public Task LogDemoGroupCreatedAsync(
        Guid epochId,
        int epochNumber,
        string displayNamePrefix,
        IReadOnlyCollection<Guid> participantIds,
        DateTime createdUtc,
        CancellationToken cancellationToken = default)
    {
        var metadataJson = JsonSerializer.Serialize(new
        {
            epochId,
            epochNumber,
            displayNamePrefix,
            participantIds,
            participantsCount = participantIds.Count
        });

        var entry = new AuditLogEntry(
            Guid.NewGuid(),
            AuditActionType.DemoGroupCreated,
            "Epoch",
            epochId,
            $"Demo group was created for epoch '{epochId}' (number {epochNumber}) with {participantIds.Count} participants.",
            metadataJson,
            createdUtc);

        return _auditLogRepository.AddAsync(entry, cancellationToken);
    }
}
