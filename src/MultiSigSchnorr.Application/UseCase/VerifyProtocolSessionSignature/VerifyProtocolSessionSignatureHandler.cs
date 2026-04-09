using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Crypto.Schnorr;

namespace MultiSigSchnorr.Application.UseCases.VerifyProtocolSessionSignature;

public sealed class VerifyProtocolSessionSignatureHandler
{
    private readonly IProtocolSessionRepository _protocolSessionRepository;
    private readonly AggregateSignatureVerifier _aggregateSignatureVerifier;

    public VerifyProtocolSessionSignatureHandler(
        IProtocolSessionRepository protocolSessionRepository,
        AggregateSignatureVerifier aggregateSignatureVerifier)
    {
        _protocolSessionRepository = protocolSessionRepository
            ?? throw new ArgumentNullException(nameof(protocolSessionRepository));
        _aggregateSignatureVerifier = aggregateSignatureVerifier
            ?? throw new ArgumentNullException(nameof(aggregateSignatureVerifier));
    }

    public async Task<VerifyProtocolSessionSignatureResult> HandleAsync(
        VerifyProtocolSessionSignatureRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.SessionId == Guid.Empty)
            throw new ArgumentException("Session id cannot be empty.", nameof(request));

        var session = await _protocolSessionRepository.GetByIdAsync(
            request.SessionId,
            cancellationToken);

        if (session is null)
            throw new InvalidOperationException(
                $"Protocol session '{request.SessionId}' was not found.");

        if (session.AggregateSignature is null)
        {
            return new VerifyProtocolSessionSignatureResult
            {
                SessionId = request.SessionId,
                IsValid = false,
                Message = "Aggregate signature is not available yet."
            };
        }

        var isValid = _aggregateSignatureVerifier.Verify(
            session.AggregateSignature,
            session.AggregatePublicKey,
            session.MessageDigest);

        return new VerifyProtocolSessionSignatureResult
        {
            SessionId = request.SessionId,
            IsValid = isValid,
            Message = isValid
                ? "Aggregate signature is valid."
                : "Aggregate signature is invalid."
        };
    }
}