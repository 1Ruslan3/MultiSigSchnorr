using Microsoft.AspNetCore.Mvc;
using MultiSigSchnorr.Application.UseCases.CreateEpochWithMembers;
using MultiSigSchnorr.Application.UseCases.CreateParticipant;
using MultiSigSchnorr.Application.UseCases.RenameParticipant;
using MultiSigSchnorr.Contracts.Administration;

namespace MultiSigSchnorr.Api.Controllers;

[ApiController]
[Route("api/admin")]
public sealed class GroupManagementController : ControllerBase
{
    private readonly CreateParticipantHandler _createParticipantHandler;
    private readonly RenameParticipantHandler _renameParticipantHandler;
    private readonly CreateEpochWithMembersHandler _createEpochWithMembersHandler;

    public GroupManagementController(
        CreateParticipantHandler createParticipantHandler,
        RenameParticipantHandler renameParticipantHandler,
        CreateEpochWithMembersHandler createEpochWithMembersHandler)
    {
        _createParticipantHandler = createParticipantHandler
            ?? throw new ArgumentNullException(nameof(createParticipantHandler));
        _renameParticipantHandler = renameParticipantHandler
            ?? throw new ArgumentNullException(nameof(renameParticipantHandler));
        _createEpochWithMembersHandler = createEpochWithMembersHandler
            ?? throw new ArgumentNullException(nameof(createEpochWithMembersHandler));
    }

    [HttpPost("participants")]
    public async Task<IActionResult> CreateParticipant(
        [FromBody] CreateParticipantApiRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _createParticipantHandler.HandleAsync(
                new CreateParticipantRequest
                {
                    DisplayName = request.DisplayName
                },
                DateTime.UtcNow,
                cancellationToken);

            return NoContent();
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("participants/{participantId:guid}/display-name")]
    public async Task<IActionResult> RenameParticipant(
        Guid participantId,
        [FromBody] RenameParticipantApiRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _renameParticipantHandler.HandleAsync(
                new RenameParticipantRequest
                {
                    ParticipantId = participantId,
                    DisplayName = request.DisplayName
                },
                DateTime.UtcNow,
                cancellationToken);

            return NoContent();
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("epochs/create-with-members")]
    public async Task<IActionResult> CreateEpochWithMembers(
        [FromBody] CreateEpochWithMembersApiRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _createEpochWithMembersHandler.HandleAsync(
                new CreateEpochWithMembersRequest
                {
                    ParticipantIds = request.ParticipantIds
                },
                DateTime.UtcNow,
                cancellationToken);

            return NoContent();
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
