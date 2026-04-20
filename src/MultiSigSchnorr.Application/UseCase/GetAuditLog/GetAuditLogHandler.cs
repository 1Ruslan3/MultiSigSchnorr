using MultiSigSchnorr.Application.Repositories;

namespace MultiSigSchnorr.Application.UseCases.GetAuditLog;

public sealed class GetAuditLogHandler
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditLogHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository
            ?? throw new ArgumentNullException(nameof(auditLogRepository));
    }

    public async Task<IReadOnlyList<AuditLogItemDto>> HandleAsync(
        GetAuditLogRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var take = request.Take <= 0 ? 100 : Math.Min(request.Take, 500);

        var entries = await _auditLogRepository.ListAsync(cancellationToken);

        var query = entries.AsEnumerable();

        if (request.ActionType.HasValue)
        {
            query = query.Where(x => x.ActionType == request.ActionType.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            query = query.Where(x =>
                string.Equals(
                    x.EntityType,
                    request.EntityType.Trim(),
                    StringComparison.OrdinalIgnoreCase));
        }

        if (request.EntityId.HasValue)
        {
            query = query.Where(x => x.EntityId == request.EntityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();

            query = query.Where(x =>
                ContainsIgnoreCase(x.Description, search) ||
                ContainsIgnoreCase(x.MetadataJson, search) ||
                ContainsIgnoreCase(x.EntityType, search) ||
                ContainsIgnoreCase(x.EntityId?.ToString(), search));
        }

        return query
            .OrderByDescending(x => x.CreatedUtc)
            .Take(take)
            .Select(x => new AuditLogItemDto
            {
                Id = x.Id,
                ActionType = x.ActionType,
                EntityType = x.EntityType,
                EntityId = x.EntityId,
                Description = x.Description,
                MetadataJson = x.MetadataJson,
                CreatedUtc = x.CreatedUtc
            })
            .ToList();
    }

    private static bool ContainsIgnoreCase(string? source, string value)
    {
        if (string.IsNullOrWhiteSpace(source))
            return false;

        return source.Contains(value, StringComparison.OrdinalIgnoreCase);
    }
}