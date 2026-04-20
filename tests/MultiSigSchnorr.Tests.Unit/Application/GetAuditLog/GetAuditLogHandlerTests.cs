using MultiSigSchnorr.Application.UseCases.GetAuditLog;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Infrastructure.Repositories;

namespace MultiSigSchnorr.Tests.Unit.Application.GetAuditLog;

public sealed class GetAuditLogHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Filter_By_ActionType()
    {
        var repository = new InMemoryAuditLogRepository();
        var handler = new GetAuditLogHandler(repository);

        await repository.AddAsync(new AuditLogEntry(
            Guid.NewGuid(),
            AuditActionType.ProtocolSessionCreated,
            "ProtocolSession",
            Guid.NewGuid(),
            "Session created",
            "{\"mode\":\"Baseline\"}",
            DateTime.UtcNow));

        await repository.AddAsync(new AuditLogEntry(
            Guid.NewGuid(),
            AuditActionType.ParticipantRevoked,
            "Participant",
            Guid.NewGuid(),
            "Participant revoked",
            "{\"reason\":\"Administrative revocation\"}",
            DateTime.UtcNow));

        var result = await handler.HandleAsync(new GetAuditLogRequest
        {
            ActionType = AuditActionType.ParticipantRevoked
        });

        Assert.Single(result);
        Assert.Equal(AuditActionType.ParticipantRevoked, result[0].ActionType);
    }

    [Fact]
    public async Task HandleAsync_Should_Filter_By_SearchTerm()
    {
        var repository = new InMemoryAuditLogRepository();
        var handler = new GetAuditLogHandler(repository);

        await repository.AddAsync(new AuditLogEntry(
            Guid.NewGuid(),
            AuditActionType.ProtocolSessionCreated,
            "ProtocolSession",
            Guid.NewGuid(),
            "Protocol session was created using mode Baseline.",
            "{\"protectionMode\":\"Baseline\"}",
            DateTime.UtcNow));

        await repository.AddAsync(new AuditLogEntry(
            Guid.NewGuid(),
            AuditActionType.ProtocolSessionCreated,
            "ProtocolSession",
            Guid.NewGuid(),
            "Protocol session was created using mode RandomizedScalarProcessing.",
            "{\"protectionMode\":\"RandomizedScalarProcessing\"}",
            DateTime.UtcNow));

        var result = await handler.HandleAsync(new GetAuditLogRequest
        {
            SearchTerm = "RandomizedScalarProcessing"
        });

        Assert.Single(result);
        Assert.Contains("RandomizedScalarProcessing", result[0].Description, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_Should_Filter_By_EntityType_And_EntityId()
    {
        var repository = new InMemoryAuditLogRepository();
        var handler = new GetAuditLogHandler(repository);

        var targetEntityId = Guid.NewGuid();

        await repository.AddAsync(new AuditLogEntry(
            Guid.NewGuid(),
            AuditActionType.EpochTransitioned,
            "Epoch",
            targetEntityId,
            "Epoch transitioned.",
            "{\"newEpochNumber\":2}",
            DateTime.UtcNow));

        await repository.AddAsync(new AuditLogEntry(
            Guid.NewGuid(),
            AuditActionType.EpochTransitioned,
            "Epoch",
            Guid.NewGuid(),
            "Another epoch transitioned.",
            "{\"newEpochNumber\":3}",
            DateTime.UtcNow));

        var result = await handler.HandleAsync(new GetAuditLogRequest
        {
            EntityType = "Epoch",
            EntityId = targetEntityId
        });

        Assert.Single(result);
        Assert.Equal(targetEntityId, result[0].EntityId);
        Assert.Equal("Epoch", result[0].EntityType);
    }
}