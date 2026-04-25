using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Protocol.Sessions;

namespace MultiSigSchnorr.Application.UseCases.RevealNonce;

public sealed class RevealNonceHandler
{
    private readonly IProtocolSessionRepository _protocolSessionRepository;
    private readonly NPartyCommitmentProtocolService _protocolService;
    private readonly IProtocolSessionProjectionRepository? _projectionRepository;

    public RevealNonceHandler(
        IProtocolSessionRepository protocolSessionRepository,
        NPartyCommitmentProtocolService protocolService,
        IProtocolSessionProjectionRepository? projectionRepository = null)
    {
        _protocolSessionRepository = protocolSessionRepository
            ?? throw new ArgumentNullException(nameof(protocolSessionRepository));
        _protocolService = protocolService
            ?? throw new ArgumentNullException(nameof(protocolService));
        _projectionRepository = projectionRepository;
    }

    public async Task<NonceReveal> HandleAsync(
        RevealNonceRequest request,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.SessionId == Guid.Empty)
            throw new ArgumentException("Session id cannot be empty.", nameof(request));

        if (request.ParticipantId == Guid.Empty)
            throw new ArgumentException("Participant id cannot be empty.", nameof(request));

        var session = await _protocolSessionRepository.GetByIdAsync(
            request.SessionId,
            cancellationToken);

        if (session is null)
            throw new InvalidOperationException(
                $"Protocol session '{request.SessionId}' was not found.");

        var reveal = _protocolService.RevealNonce(
            session,
            request.ParticipantId,
            nowUtc);

        await _protocolSessionRepository.UpdateAsync(session, cancellationToken);

        if (_projectionRepository is not null)
            await _projectionRepository.UpsertAsync(session, cancellationToken);

        return reveal;
    }
}