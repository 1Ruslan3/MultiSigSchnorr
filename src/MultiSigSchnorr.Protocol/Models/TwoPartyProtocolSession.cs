using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Protocol.Models;

public sealed class TwoPartyProtocolSession
{
    public SigningSession SigningSession { get; }
    public MessageDigestValue MessageDigest { get; }
    public PublicKeyValue AggregatePublicKey { get; }

    public TwoPartyParticipantProtocolState FirstParticipant { get; }
    public TwoPartyParticipantProtocolState SecondParticipant { get; }

    public PointValue? AggregateNoncePoint { get; private set; }
    public ScalarValue? Challenge { get; private set; }
    public AggregateSignature? AggregateSignature { get; private set; }

    public Guid SessionId => SigningSession.Id;

    public bool AllCommitmentsPublished =>
        FirstParticipant.HasCommitment && SecondParticipant.HasCommitment;

    public bool AllNoncesRevealed =>
        FirstParticipant.HasReveal && SecondParticipant.HasReveal;

    public bool AllPartialSignaturesSubmitted =>
        FirstParticipant.HasPartialSignature && SecondParticipant.HasPartialSignature;

    public TwoPartyProtocolSession(
        SigningSession signingSession,
        MessageDigestValue messageDigest,
        PublicKeyValue aggregatePublicKey,
        TwoPartyParticipantProtocolState firstParticipant,
        TwoPartyParticipantProtocolState secondParticipant)
    {
        SigningSession = signingSession ?? throw new ArgumentNullException(nameof(signingSession));
        MessageDigest = messageDigest ?? throw new ArgumentNullException(nameof(messageDigest));
        AggregatePublicKey = aggregatePublicKey ?? throw new ArgumentNullException(nameof(aggregatePublicKey));
        FirstParticipant = firstParticipant ?? throw new ArgumentNullException(nameof(firstParticipant));
        SecondParticipant = secondParticipant ?? throw new ArgumentNullException(nameof(secondParticipant));
    }

    public TwoPartyParticipantProtocolState GetParticipant(Guid participantId)
    {
        if (participantId == FirstParticipant.ParticipantId)
            return FirstParticipant;

        if (participantId == SecondParticipant.ParticipantId)
            return SecondParticipant;

        throw new InvalidOperationException("Participant is not part of this two-party session.");
    }

    public void SetAggregateNoncePoint(PointValue aggregateNoncePoint)
    {
        AggregateNoncePoint = aggregateNoncePoint ?? throw new ArgumentNullException(nameof(aggregateNoncePoint));
    }

    public void SetChallenge(ScalarValue challenge)
    {
        Challenge = challenge ?? throw new ArgumentNullException(nameof(challenge));
    }

    public void SetAggregateSignature(AggregateSignature aggregateSignature)
    {
        AggregateSignature = aggregateSignature ?? throw new ArgumentNullException(nameof(aggregateSignature));
    }
}