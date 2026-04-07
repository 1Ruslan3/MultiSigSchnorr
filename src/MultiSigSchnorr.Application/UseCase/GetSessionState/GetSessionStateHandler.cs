using MultiSigSchnorr.Application.Repositories;

namespace MultiSigSchnorr.Application.UseCases.GetSessionState;

public sealed class GetSessionStateHandler
{
    private readonly IProtocolSessionRepository _protocolSessionRepository;

    public GetSessionStateHandler(IProtocolSessionRepository protocolSessionRepository)
    {
        _protocolSessionRepository = protocolSessionRepository
            ?? throw new ArgumentNullException(nameof(protocolSessionRepository));
    }

    public async Task<SessionStateDto> HandleAsync(
        GetSessionStateRequest request,
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

        var participantDtos = session.Participants.Values
            .OrderBy(x => x.DisplayName, StringComparer.Ordinal)
            .Select(x => new SessionParticipantStateDto
            {
                ParticipantId = x.ParticipantId,
                DisplayName = x.DisplayName,
                HasCommitment = x.HasCommitment,
                HasReveal = x.HasReveal,
                HasPartialSignature = x.HasPartialSignature,
                PublicKeyHex = x.PublicKey.ToHex(),
                AggregationCoefficientHex = x.AggregationCoefficient.ToHex(),
                CommitmentHex = x.CommitmentRecord?.Commitment.ToHex(),
                PublicNoncePointHex = x.RevealRecord?.PublicNoncePoint.ToHex(),
                PartialSignatureHex = x.PartialSignatureRecord?.SignatureScalar.ToHex()
            })
            .ToList();

        return new SessionStateDto
        {
            SessionId = session.SessionId,
            EpochId = session.Epoch.Id,
            EpochNumber = session.Epoch.Number,
            SessionStatus = session.SigningSession.Status,
            MessageDigestHex = session.MessageDigest.ToHex(),
            AggregatePublicKeyHex = session.AggregatePublicKey.ToHex(),
            AggregateNoncePointHex = session.AggregateNoncePoint?.ToHex(),
            ChallengeHex = session.Challenge?.ToHex(),
            AggregateSignatureNoncePointHex = session.AggregateSignature?.AggregateNoncePoint.ToHex(),
            AggregateSignatureScalarHex = session.AggregateSignature?.SignatureScalar.ToHex(),
            AllCommitmentsPublished = session.AllCommitmentsPublished,
            AllNoncesRevealed = session.AllNoncesRevealed,
            AllPartialSignaturesSubmitted = session.AllPartialSignaturesSubmitted,
            Participants = participantDtos
        };
    }
}