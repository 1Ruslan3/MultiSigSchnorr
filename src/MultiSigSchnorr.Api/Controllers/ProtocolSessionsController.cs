using Microsoft.AspNetCore.Mvc;
using MultiSigSchnorr.Application.UseCases.CreateProtocolSession;
using MultiSigSchnorr.Application.UseCases.GetSessionState;
using MultiSigSchnorr.Application.UseCases.PublishCommitment;
using MultiSigSchnorr.Application.UseCases.RevealNonce;
using MultiSigSchnorr.Application.UseCases.SubmitPartialSignature;
using MultiSigSchnorr.Contracts.ProtocolSessions;

namespace MultiSigSchnorr.Api.Controllers;

[ApiController]
[Route("api/protocol-sessions")]
public sealed class ProtocolSessionsController : ControllerBase
{
    private readonly CreateProtocolSessionHandler _createProtocolSessionHandler;
    private readonly PublishCommitmentHandler _publishCommitmentHandler;
    private readonly RevealNonceHandler _revealNonceHandler;
    private readonly SubmitPartialSignatureHandler _submitPartialSignatureHandler;
    private readonly GetSessionStateHandler _getSessionStateHandler;

    public ProtocolSessionsController(
        CreateProtocolSessionHandler createProtocolSessionHandler,
        PublishCommitmentHandler publishCommitmentHandler,
        RevealNonceHandler revealNonceHandler,
        SubmitPartialSignatureHandler submitPartialSignatureHandler,
        GetSessionStateHandler getSessionStateHandler)
    {
        _createProtocolSessionHandler = createProtocolSessionHandler;
        _publishCommitmentHandler = publishCommitmentHandler;
        _revealNonceHandler = revealNonceHandler;
        _submitPartialSignatureHandler = submitPartialSignatureHandler;
        _getSessionStateHandler = getSessionStateHandler;
    }

    [HttpPost]
    public async Task<ActionResult<SessionStateApiResponse>> Create(
        [FromBody] CreateProtocolSessionApiRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var session = await _createProtocolSessionHandler.HandleAsync(
                new CreateProtocolSessionRequest
                {
                    EpochId = request.EpochId,
                    ParticipantIds = request.ParticipantIds,
                    Message = request.Message
                },
                DateTime.UtcNow,
                cancellationToken);

            var state = await _getSessionStateHandler.HandleAsync(
                new GetSessionStateRequest
                {
                    SessionId = session.SessionId
                },
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = state.SessionId },
                Map(state));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/commitments")]
    public async Task<ActionResult<SessionStateApiResponse>> PublishCommitment(
        Guid id,
        [FromBody] PublishCommitmentApiRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _publishCommitmentHandler.HandleAsync(
                new PublishCommitmentRequest
                {
                    SessionId = id,
                    ParticipantId = request.ParticipantId
                },
                DateTime.UtcNow,
                cancellationToken);

            var state = await _getSessionStateHandler.HandleAsync(
                new GetSessionStateRequest { SessionId = id },
                cancellationToken);

            return Ok(Map(state));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/reveals")]
    public async Task<ActionResult<SessionStateApiResponse>> RevealNonce(
        Guid id,
        [FromBody] RevealNonceApiRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _revealNonceHandler.HandleAsync(
                new RevealNonceRequest
                {
                    SessionId = id,
                    ParticipantId = request.ParticipantId
                },
                DateTime.UtcNow,
                cancellationToken);

            var state = await _getSessionStateHandler.HandleAsync(
                new GetSessionStateRequest { SessionId = id },
                cancellationToken);

            return Ok(Map(state));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/partial-signatures")]
    public async Task<ActionResult<SessionStateApiResponse>> SubmitPartialSignature(
        Guid id,
        [FromBody] SubmitPartialSignatureApiRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _submitPartialSignatureHandler.HandleAsync(
                new SubmitPartialSignatureRequest
                {
                    SessionId = id,
                    ParticipantId = request.ParticipantId
                },
                DateTime.UtcNow,
                cancellationToken);

            var state = await _getSessionStateHandler.HandleAsync(
                new GetSessionStateRequest { SessionId = id },
                cancellationToken);

            return Ok(Map(state));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SessionStateApiResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var state = await _getSessionStateHandler.HandleAsync(
                new GetSessionStateRequest { SessionId = id },
                cancellationToken);

            return Ok(Map(state));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    private static SessionStateApiResponse Map(SessionStateDto dto)
    {
        return new SessionStateApiResponse
        {
            SessionId = dto.SessionId,
            EpochId = dto.EpochId,
            EpochNumber = dto.EpochNumber,
            SessionStatus = dto.SessionStatus,
            MessageDigestHex = dto.MessageDigestHex,
            AggregatePublicKeyHex = dto.AggregatePublicKeyHex,
            AggregateNoncePointHex = dto.AggregateNoncePointHex,
            ChallengeHex = dto.ChallengeHex,
            AggregateSignatureNoncePointHex = dto.AggregateSignatureNoncePointHex,
            AggregateSignatureScalarHex = dto.AggregateSignatureScalarHex,
            AllCommitmentsPublished = dto.AllCommitmentsPublished,
            AllNoncesRevealed = dto.AllNoncesRevealed,
            AllPartialSignaturesSubmitted = dto.AllPartialSignaturesSubmitted,
            Participants = dto.Participants
                .Select(p => new SessionParticipantStateApiResponse
                {
                    ParticipantId = p.ParticipantId,
                    DisplayName = p.DisplayName,
                    HasCommitment = p.HasCommitment,
                    HasReveal = p.HasReveal,
                    HasPartialSignature = p.HasPartialSignature,
                    PublicKeyHex = p.PublicKeyHex,
                    AggregationCoefficientHex = p.AggregationCoefficientHex,
                    CommitmentHex = p.CommitmentHex,
                    PublicNoncePointHex = p.PublicNoncePointHex,
                    PartialSignatureHex = p.PartialSignatureHex
                })
                .ToList()
        };
    }
}