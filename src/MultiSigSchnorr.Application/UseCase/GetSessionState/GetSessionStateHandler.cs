using MultiSigSchnorr.Protocol.Models;

namespace MultiSigSchnorr.Application.UseCases.GetSessionState;

public sealed class GetSessionStateHandler
{
    public SessionStateDto Handle(
        GetSessionStateRequest request,
        NPartyProtocolSession session)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(session);

        if (request.SessionId == Guid.Empty)
            throw new ArgumentException("Session id cannot be empty.", nameof(request));

        if (request.SessionId != session.SessionId)
            throw new InvalidOperationException("Request session id does not match the provided protocol session.");

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