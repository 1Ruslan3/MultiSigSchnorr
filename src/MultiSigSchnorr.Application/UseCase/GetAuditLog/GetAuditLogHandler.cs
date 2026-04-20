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

        return entries
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
}