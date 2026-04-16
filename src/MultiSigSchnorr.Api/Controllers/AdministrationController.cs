using Microsoft.AspNetCore.Mvc;
using MultiSigSchnorr.Application.UseCases.GetEpochAdministrationState;
using MultiSigSchnorr.Application.UseCases.RevokeParticipantInActiveEpoch;
using MultiSigSchnorr.Application.UseCases.TransitionToNextEpoch;
using MultiSigSchnorr.Contracts.Administration;

namespace MultiSigSchnorr.Api.Controllers;

[ApiController]
[Route("api/admin")]
public sealed class AdministrationController : ControllerBase
{
    private readonly GetEpochAdministrationStateHandler _getEpochAdministrationStateHandler;
    private readonly RevokeParticipantInActiveEpochHandler _revokeParticipantInActiveEpochHandler;
    private readonly TransitionToNextEpochHandler _transitionToNextEpochHandler;

    public AdministrationController(
        GetEpochAdministrationStateHandler getEpochAdministrationStateHandler,
        RevokeParticipantInActiveEpochHandler revokeParticipantInActiveEpochHandler,
        TransitionToNextEpochHandler transitionToNextEpochHandler)
    {
        _getEpochAdministrationStateHandler = getEpochAdministrationStateHandler;
        _revokeParticipantInActiveEpochHandler = revokeParticipantInActiveEpochHandler;
        _transitionToNextEpochHandler = transitionToNextEpochHandler;
    }

    [HttpGet("epoch-management")]
    public async Task<ActionResult<EpochAdministrationStateApiResponse>> GetState(
        CancellationToken cancellationToken)
    {
        try
        {
            var state = await _getEpochAdministrationStateHandler.HandleAsync(
                new GetEpochAdministrationStateRequest(),
                cancellationToken);

            return Ok(Map(state));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("participants/{participantId:guid}/revoke")]
    public async Task<ActionResult<EpochAdministrationStateApiResponse>> RevokeParticipant(
        Guid participantId,
        [FromBody] RevokeParticipantApiRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _revokeParticipantInActiveEpochHandler.HandleAsync(
                new RevokeParticipantInActiveEpochRequest
                {
                    ParticipantId = participantId,
                    Reason = request.Reason
                },
                DateTime.UtcNow,
                cancellationToken);

            var state = await _getEpochAdministrationStateHandler.HandleAsync(
                new GetEpochAdministrationStateRequest(),
                cancellationToken);

            return Ok(Map(state));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("epochs/transition")]
    public async Task<ActionResult<EpochAdministrationStateApiResponse>> TransitionEpoch(
        CancellationToken cancellationToken)
    {
        try
        {
            await _transitionToNextEpochHandler.HandleAsync(
                new TransitionToNextEpochRequest(),
                DateTime.UtcNow,
                cancellationToken);

            var state = await _getEpochAdministrationStateHandler.HandleAsync(
                new GetEpochAdministrationStateRequest(),
                cancellationToken);

            return Ok(Map(state));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private static EpochAdministrationStateApiResponse Map(EpochAdministrationStateDto dto)
    {
        return new EpochAdministrationStateApiResponse
        {
            ActiveEpochId = dto.ActiveEpochId,
            ActiveEpochNumber = dto.ActiveEpochNumber,
            ActiveEpochStatus = dto.ActiveEpochStatus,
            Epochs = dto.Epochs
                .Select(x => new EpochAdministrationEpochApiResponse
                {
                    EpochId = x.EpochId,
                    EpochNumber = x.EpochNumber,
                    EpochStatus = x.EpochStatus
                })
                .ToList(),
            Participants = dto.Participants
                .Select(x => new EpochAdministrationParticipantApiResponse
                {
                    ParticipantId = x.ParticipantId,
                    DisplayName = x.DisplayName,
                    ParticipantStatus = x.ParticipantStatus,
                    PublicKeyHex = x.PublicKeyHex,
                    IsMemberOfActiveEpoch = x.IsMemberOfActiveEpoch,
                    IsActiveMemberOfActiveEpoch = x.IsActiveMemberOfActiveEpoch,
                    CanBeRevoked = x.CanBeRevoked
                })
                .ToList()
        };
    }
}