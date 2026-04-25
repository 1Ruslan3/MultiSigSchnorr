namespace MultiSigSchnorr.Contracts.Diagnostics;

public sealed class StorageDiagnosticsApiResponse
{
    public string StorageProvider { get; init; } = string.Empty;
    public string DatabaseProvider { get; init; } = string.Empty;
    public bool CanConnect { get; init; }

    public int AppliedMigrationsCount { get; init; }
    public string? LatestMigration { get; init; }

    public int EpochsCount { get; init; }
    public int ParticipantsCount { get; init; }
    public int EpochMembersCount { get; init; }
    public int AuditLogEntriesCount { get; init; }
    public int ProtocolSessionsCount { get; init; }
    public int ProtocolSessionParticipantsCount { get; init; }

    public DateTime CheckedUtc { get; init; }
}