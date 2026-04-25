using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiSigSchnorr.Contracts.Diagnostics;
using MultiSigSchnorr.Infrastructure.Persistence;

namespace MultiSigSchnorr.Api.Controllers;

[ApiController]
[Route("api/system/storage")]
public sealed class StorageDiagnosticsController : ControllerBase
{
    private readonly MultiSigSchnorrDbContext _dbContext;

    public StorageDiagnosticsController(MultiSigSchnorrDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    [HttpGet]
    public async Task<ActionResult<StorageDiagnosticsApiResponse>> GetStorageDiagnostics(
        CancellationToken cancellationToken)
    {
        var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

        var appliedMigrations = canConnect
            ? await _dbContext.Database.GetAppliedMigrationsAsync(cancellationToken)
            : Array.Empty<string>();

        var migrationsList = appliedMigrations.ToList();

        var response = new StorageDiagnosticsApiResponse
        {
            StorageProvider = "PostgreSQL + EF Core",
            DatabaseProvider = _dbContext.Database.ProviderName ?? "Unknown",
            CanConnect = canConnect,
            AppliedMigrationsCount = migrationsList.Count,
            LatestMigration = migrationsList.LastOrDefault(),

            EpochsCount = canConnect
                ? await _dbContext.Epochs.CountAsync(cancellationToken)
                : 0,

            ParticipantsCount = canConnect
                ? await _dbContext.Participants.CountAsync(cancellationToken)
                : 0,

            EpochMembersCount = canConnect
                ? await _dbContext.EpochMembers.CountAsync(cancellationToken)
                : 0,

            AuditLogEntriesCount = canConnect
                ? await _dbContext.AuditLogEntries.CountAsync(cancellationToken)
                : 0,

            ProtocolSessionsCount = canConnect
                ? await _dbContext.ProtocolSessions.CountAsync(cancellationToken)
                : 0,

            ProtocolSessionParticipantsCount = canConnect
                ? await _dbContext.ProtocolSessionParticipants.CountAsync(cancellationToken)
                : 0,

            CheckedUtc = DateTime.UtcNow
        };

        return Ok(response);
    }
}