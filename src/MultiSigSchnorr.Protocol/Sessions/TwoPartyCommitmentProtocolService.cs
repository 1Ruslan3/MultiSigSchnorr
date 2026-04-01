using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Crypto.Aggregation;
using MultiSigSchnorr.Crypto.Security;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.ValueObjects;
using MultiSigSchnorr.Protocol.Models;

namespace MultiSigSchnorr.Protocol.Sessions;

public sealed class TwoPartyCommitmentProtocolService
{
    private readonly IPublicKeyGenerationService _publicKeyGenerationService;
    private readonly IAggregateKeyService _aggregateKeyService;
    private readonly INonceGenerator _nonceGenerator;
    private readonly ICommitmentService _commitmentService;
    private readonly IChallengeService _challengeService;
    private readonly IPartialSignatureService _partialSignatureService;
    private readonly IAggregateSignatureVerifier _aggregateSignatureVerifier;
    private readonly ICurveContext _curveContext;

    public TwoPartyCommitmentProtocolService(
        IPublicKeyGenerationService publicKeyGenerationService,
        IAggregateKeyService aggregateKeyService,
        INonceGenerator nonceGenerator,
        ICommitmentService commitmentService,
        IChallengeService challengeService,
        IPartialSignatureService partialSignatureService,
        IAggregateSignatureVerifier aggregateSignatureVerifier,
        ICurveContext curveContext)
    {
        _publicKeyGenerationService = publicKeyGenerationService ?? throw new ArgumentNullException(nameof(publicKeyGenerationService));
        _aggregateKeyService = aggregateKeyService ?? throw new ArgumentNullException(nameof(aggregateKeyService));
        _nonceGenerator = nonceGenerator ?? throw new ArgumentNullException(nameof(nonceGenerator));
        _commitmentService = commitmentService ?? throw new ArgumentNullException(nameof(commitmentService));
        _challengeService = challengeService ?? throw new ArgumentNullException(nameof(challengeService));
        _partialSignatureService = partialSignatureService ?? throw new ArgumentNullException(nameof(partialSignatureService));
        _aggregateSignatureVerifier = aggregateSignatureVerifier ?? throw new ArgumentNullException(nameof(aggregateSignatureVerifier));
        _curveContext = curveContext ?? throw new ArgumentNullException(nameof(curveContext));
    }

    public TwoPartyProtocolSession CreateSession(
        Guid epochId,
        ScalarValue firstPrivateKey,
        ScalarValue secondPrivateKey,
        MessageDigestValue messageDigest,
        DateTime createdUtc)
    {
        if (epochId == Guid.Empty)
            throw new ArgumentException("Epoch id cannot be empty.", nameof(epochId));

        ArgumentNullException.ThrowIfNull(firstPrivateKey);
        ArgumentNullException.ThrowIfNull(secondPrivateKey);
        ArgumentNullException.ThrowIfNull(messageDigest);

        var firstPublicKey = _publicKeyGenerationService.DerivePublicKey(firstPrivateKey);
        var secondPublicKey = _publicKeyGenerationService.DerivePublicKey(secondPrivateKey);

        var aggregateKeyResult = _aggregateKeyService.Compute(
            new[] { firstPublicKey, secondPublicKey });

        var firstParticipant = new TwoPartyParticipantProtocolState(
            Guid.NewGuid(),
            firstPrivateKey,
            firstPublicKey,
            aggregateKeyResult.GetCoefficient(firstPublicKey));

        var secondParticipant = new TwoPartyParticipantProtocolState(
            Guid.NewGuid(),
            secondPrivateKey,
            secondPublicKey,
            aggregateKeyResult.GetCoefficient(secondPublicKey));

        var signingSession = new SigningSession(
            Guid.NewGuid(),
            epochId,
            messageDigest.ToHex(),
            createdUtc);

        return new TwoPartyProtocolSession(
            signingSession,
            messageDigest,
            aggregateKeyResult.AggregatePublicKey,
            firstParticipant,
            secondParticipant);
    }

    public NonceCommitment PublishCommitment(
        TwoPartyProtocolSession session,
        Guid participantId,
        DateTime submittedUtc)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (session.SigningSession.Status == Domain.Enums.SessionStatus.Created)
            session.SigningSession.StartCommitmentsCollection();

        var participant = session.GetParticipant(participantId);

        var commitment = participant.CreateCommitment(
            session.SessionId,
            submittedUtc,
            _nonceGenerator,
            _commitmentService);

        if (session.AllCommitmentsPublished)
            session.SigningSession.StartNonceRevealCollection();

        return commitment;
    }

    public NonceReveal RevealNonce(
        TwoPartyProtocolSession session,
        Guid participantId,
        DateTime submittedUtc)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (!session.AllCommitmentsPublished)
            throw new InvalidOperationException("All commitments must be published before nonce reveal.");

        var participant = session.GetParticipant(participantId);

        var reveal = participant.RevealNonce(
            session.SessionId,
            submittedUtc,
            _commitmentService);

        if (session.AllNoncesRevealed)
        {
            var aggregateNoncePoint = _curveContext.AddPoints(
                session.FirstParticipant.RevealRecord!.PublicNoncePoint,
                session.SecondParticipant.RevealRecord!.PublicNoncePoint);

            session.SetAggregateNoncePoint(aggregateNoncePoint);

            var challenge = _challengeService.ComputeChallenge(
                aggregateNoncePoint,
                session.AggregatePublicKey,
                session.MessageDigest);

            session.SetChallenge(challenge);
            session.SigningSession.StartPartialSignaturesCollection();
        }

        return reveal;
    }

    public PartialSignature SubmitPartialSignature(
        TwoPartyProtocolSession session,
        Guid participantId,
        DateTime submittedUtc)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (!session.AllNoncesRevealed || session.Challenge is null)
            throw new InvalidOperationException("All nonces must be revealed before partial signature submission.");

        var participant = session.GetParticipant(participantId);

        var partialSignature = participant.CreatePartialSignature(
            session.SessionId,
            submittedUtc,
            _partialSignatureService,
            session.Challenge);

        if (session.AllPartialSignaturesSubmitted)
        {
            var aggregateScalar = ScalarMath.AddMod(
                _curveContext,
                session.FirstParticipant.PartialSignatureRecord!.SignatureScalar.Value,
                session.SecondParticipant.PartialSignatureRecord!.SignatureScalar.Value);

            var aggregateSignature = new AggregateSignature(
                Guid.NewGuid(),
                session.SessionId,
                session.AggregateNoncePoint!,
                new SignatureScalarValue(aggregateScalar),
                submittedUtc);

            if (!_aggregateSignatureVerifier.Verify(
                    aggregateSignature,
                    session.AggregatePublicKey,
                    session.MessageDigest))
            {
                session.SigningSession.Fail();
                throw new InvalidOperationException("Aggregate signature verification failed during session finalization.");
            }

            session.SetAggregateSignature(aggregateSignature);
            session.SigningSession.Complete(submittedUtc);
        }

        return partialSignature;
    }
}