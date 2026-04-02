using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Protocol.Models;
using MultiSigSchnorr.Protocol.Sessions;

namespace MultiSigSchnorr.Application.UseCases.SubmitPartialSignature;

public sealed class SubmitPartialSignatureHandler
{
    private readonly NPartyCommitmentProtocolService _protocolService;

    public SubmitPartialSignatureHandler(NPartyCommitmentProtocolService protocolService)
    {
        _protocolService = protocolService ?? throw new ArgumentNullException(nameof(protocolService));
    }

    public PartialSignature Handle(
        SubmitPartialSignatureRequest request,
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

        return _protocolService.SubmitPartialSignature(
            session,
            request.ParticipantId,
            nowUtc);
    }
}