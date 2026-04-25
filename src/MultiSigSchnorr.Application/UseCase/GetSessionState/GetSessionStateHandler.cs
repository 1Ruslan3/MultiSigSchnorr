using MultiSigSchnorr.Application.Projections;
using MultiSigSchnorr.Application.Repositories;

namespace MultiSigSchnorr.Application.UseCases.GetSessionState;

public sealed class GetSessionStateHandler
{
    private readonly IProtocolSessionRepository _protocolSessionRepository;
    private readonly IProtocolSessionProjectionRepository? _projectionRepository;

    public GetSessionStateHandler(
        IProtocolSessionRepository protocolSessionRepository,
        IProtocolSessionProjectionRepository? projectionRepository = null)
    {
        _protocolSessionRepository = protocolSessionRepository
            ?? throw new ArgumentNullException(nameof(protocolSessionRepository));
        _projectionRepository = projectionRepository;
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

        if (session is not null)
        {
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
                ProtectionMode = session.ProtectionMode,
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

        if (_projectionRepository is not null)
        {
            var projection = await _projectionRepository.GetByIdAsync(
                request.SessionId,
                cancellationToken);

            if (projection is not null)
                return MapFromProjection(projection);
        }

        throw new InvalidOperationException(
            $"Protocol session '{request.SessionId}' was not found.");
    }

    private static SessionStateDto MapFromProjection(ProtocolSessionProjection projection)
    {
        return new SessionStateDto
        {
            SessionId = projection.SessionId,
            EpochId = projection.EpochId,
            EpochNumber = projection.EpochNumber,
            SessionStatus = projection.SessionStatus,
            ProtectionMode = projection.ProtectionMode,
            MessageDigestHex = projection.MessageDigestHex,
            AggregatePublicKeyHex = projection.AggregatePublicKeyHex,
            AggregateNoncePointHex = projection.AggregateNoncePointHex,
            ChallengeHex = projection.ChallengeHex,
            AggregateSignatureNoncePointHex = projection.AggregateSignatureNoncePointHex,
            AggregateSignatureScalarHex = projection.AggregateSignatureScalarHex,
            AllCommitmentsPublished = projection.AllCommitmentsPublished,
            AllNoncesRevealed = projection.AllNoncesRevealed,
            AllPartialSignaturesSubmitted = projection.AllPartialSignaturesSubmitted,
            Participants = projection.Participants
                .Select(x => new SessionParticipantStateDto
                {
                    ParticipantId = x.ParticipantId,
                    DisplayName = x.DisplayName,
                    HasCommitment = x.HasCommitment,
                    HasReveal = x.HasReveal,
                    HasPartialSignature = x.HasPartialSignature,
                    PublicKeyHex = x.PublicKeyHex,
                    AggregationCoefficientHex = x.AggregationCoefficientHex,
                    CommitmentHex = x.CommitmentHex,
                    PublicNoncePointHex = x.PublicNoncePointHex,
                    PartialSignatureHex = x.PartialSignatureHex
                })
                .ToList()
        };
    }
}