using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Protocol.Models;
using MultiSigSchnorr.Protocol.Sessions;

namespace MultiSigSchnorr.Application.UseCases.RevealNonce;

public sealed class RevealNonceHandler
{
    private readonly NPartyCommitmentProtocolService _protocolService;

    public RevealNonceHandler(NPartyCommitmentProtocolService protocolService)
    {
        _protocolService = protocolService ?? throw new ArgumentNullException(nameof(protocolService));
    }

    public NonceReveal Handle(
        RevealNonceRequest request,
        NPartyProtocolSession session,
        DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);

        if (request.SessionId == Guid.Empty)
            throw new ArgumentException("Session id cannot be empty.", nameof(request));

        if (request.ParticipantId == Guid.Empty)
            throw new ArgumentException("Participant id cannot be empty.", nameof(request));

        if (request.SessionId != session.SessionId)
            throw new InvalidOperationException("Request session id does not match the provided protocol session.");

        return _protocolService.RevealNonce(
            session,
            request.ParticipantId,
            nowUtc);
    }
}