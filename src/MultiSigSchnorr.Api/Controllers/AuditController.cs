using Microsoft.AspNetCore.Mvc;
using MultiSigSchnorr.Application.UseCases.GetAuditLog;
using MultiSigSchnorr.Contracts.Audit;

namespace MultiSigSchnorr.Api.Controllers;

[ApiController]
[Route("api/audit")]
public sealed class AuditController : ControllerBase
{
    private readonly GetAuditLogHandler _getAuditLogHandler;

    public AuditController(GetAuditLogHandler getAuditLogHandler)
    {
        _getAuditLogHandler = getAuditLogHandler
            ?? throw new ArgumentNullException(nameof(getAuditLogHandler));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditLogItemApiResponse>>> GetAuditLog(
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var items = await _getAuditLogHandler.HandleAsync(
            new GetAuditLogRequest
            {
                Take = take
            },
            cancellationToken);

        return Ok(items.Select(x => new AuditLogItemApiResponse
        {
            Id = x.Id,
            ActionType = x.ActionType,
            EntityType = x.EntityType,
            EntityId = x.EntityId,
            Description = x.Description,
            MetadataJson = x.MetadataJson,
            CreatedUtc = x.CreatedUtc
        }).ToList());
    }
}