using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Entities;

namespace MultiSigSchnorr.Application.UseCases.PublishCommitment;

public sealed class PublishCommitmentHandler
{
    private readonly IProtocolSessionRepository _protocolSessionRepository;
    private readonly MultiSigSchnorr.Protocol.Sessions.NPartyCommitmentProtocolService _protocolService;

    public PublishCommitmentHandler(
        IProtocolSessionRepository protocolSessionRepository,
        MultiSigSchnorr.Protocol.Sessions.NPartyCommitmentProtocolService protocolService)
    {
        _protocolSessionRepository = protocolSessionRepository
            ?? throw new ArgumentNullException(nameof(protocolSessionRepository));
        _protocolService = protocolService
            ?? throw new ArgumentNullException(nameof(protocolService));
    }

    public async Task<NonceCommitment> HandleAsync(
        PublishCommitmentRequest request,
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

        var commitment = _protocolService.PublishCommitment(
            session,
            request.ParticipantId,
            nowUtc);

        await _protocolSessionRepository.UpdateAsync(session, cancellationToken);

        return commitment;
    }
}