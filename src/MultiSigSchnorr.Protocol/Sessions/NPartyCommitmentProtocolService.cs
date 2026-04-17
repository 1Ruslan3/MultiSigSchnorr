using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Crypto.Security;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Domain.ValueObjects;
using MultiSigSchnorr.Protocol.Epochs;
using MultiSigSchnorr.Protocol.Models;

namespace MultiSigSchnorr.Protocol.Sessions;

public sealed class NPartyCommitmentProtocolService
{
    private readonly IPublicKeyGenerationService _publicKeyGenerationService;
    private readonly IAggregateKeyService _aggregateKeyService;
    private readonly INonceGenerator _nonceGenerator;
    private readonly ICommitmentService _commitmentService;
    private readonly IChallengeService _challengeService;
    private readonly IPartialSignatureService _partialSignatureService;
    private readonly IAggregateSignatureVerifier _aggregateSignatureVerifier;
    private readonly ICurveContext _curveContext;
    private readonly EpochParticipationGuard _epochParticipationGuard;

    public NPartyCommitmentProtocolService(
        IPublicKeyGenerationService publicKeyGenerationService,
        IAggregateKeyService aggregateKeyService,
        INonceGenerator nonceGenerator,
        ICommitmentService commitmentService,
        IChallengeService challengeService,
        IPartialSignatureService partialSignatureService,
        IAggregateSignatureVerifier aggregateSignatureVerifier,
        ICurveContext curveContext,
        EpochParticipationGuard epochParticipationGuard)
    {
        _publicKeyGenerationService = publicKeyGenerationService ?? throw new ArgumentNullException(nameof(publicKeyGenerationService));
        _aggregateKeyService = aggregateKeyService ?? throw new ArgumentNullException(nameof(aggregateKeyService));
        _nonceGenerator = nonceGenerator ?? throw new ArgumentNullException(nameof(nonceGenerator));
        _commitmentService = commitmentService ?? throw new ArgumentNullException(nameof(commitmentService));
        _challengeService = challengeService ?? throw new ArgumentNullException(nameof(challengeService));
        _partialSignatureService = partialSignatureService ?? throw new ArgumentNullException(nameof(partialSignatureService));
        _aggregateSignatureVerifier = aggregateSignatureVerifier ?? throw new ArgumentNullException(nameof(aggregateSignatureVerifier));
        _curveContext = curveContext ?? throw new ArgumentNullException(nameof(curveContext));
        _epochParticipationGuard = epochParticipationGuard ?? throw new ArgumentNullException(nameof(epochParticipationGuard));
    }

    public NPartyProtocolSession CreateSession(
        Epoch epoch,
        IReadOnlyList<Participant> participants,
        IReadOnlyList<EpochMember> epochMembers,
        IReadOnlyDictionary<Guid, ScalarValue> privateKeys,
        MessageDigestValue messageDigest,
        DateTime createdUtc,
        SignatureProtectionMode protectionMode = SignatureProtectionMode.Baseline)
    {
        ArgumentNullException.ThrowIfNull(epoch);
        ArgumentNullException.ThrowIfNull(participants);
        ArgumentNullException.ThrowIfNull(epochMembers);
        ArgumentNullException.ThrowIfNull(privateKeys);
        ArgumentNullException.ThrowIfNull(messageDigest);

        _epochParticipationGuard.EnsureSessionCanBeCreated(epoch, participants, epochMembers);

        var distinctParticipants = participants
            .GroupBy(x => x.Id)
            .Select(g => g.First())
            .ToList();

        if (distinctParticipants.Count != participants.Count)
            throw new InvalidOperationException("Duplicate participants are not allowed in the signing session.");

        foreach (var participant in distinctParticipants)
        {
            if (!privateKeys.ContainsKey(participant.Id))
            {
                throw new InvalidOperationException(
                    $"Private key for participant '{participant.DisplayName}' was not provided.");
            }
        }

        var publicKeys = new List<PublicKeyValue>(distinctParticipants.Count);

        foreach (var participant in distinctParticipants)
        {
            var derivedPublicKey = _publicKeyGenerationService.DerivePublicKey(
                privateKeys[participant.Id],
                protectionMode);

            if (!derivedPublicKey.Equals(participant.PublicKey))
            {
                throw new InvalidOperationException(
                    $"Derived public key does not match the registered public key for participant '{participant.DisplayName}'.");
            }

            publicKeys.Add(participant.PublicKey);
        }

        var aggregateKeyResult = _aggregateKeyService.Compute(publicKeys);

        var states = new Dictionary<Guid, NPartyParticipantProtocolState>();
        var sessionId = Guid.NewGuid();
        var sessionMembers = new List<SessionMember>(distinctParticipants.Count);

        foreach (var participant in distinctParticipants)
        {
            var coefficient = aggregateKeyResult.GetCoefficient(participant.PublicKey);

            states[participant.Id] = new NPartyParticipantProtocolState(
                participant.Id,
                participant.DisplayName,
                privateKeys[participant.Id],
                participant.PublicKey,
                coefficient);

            sessionMembers.Add(new SessionMember(
                Guid.NewGuid(),
                sessionId,
                participant.Id,
                createdUtc));
        }

        var signingSession = new SigningSession(
            sessionId,
            epoch.Id,
            messageDigest.ToHex(),
            createdUtc);

        return new NPartyProtocolSession(
            epoch,
            signingSession,
            messageDigest,
            aggregateKeyResult.AggregatePublicKey,
            states,
            sessionMembers,
            protectionMode);
    }

    public NonceCommitment PublishCommitment(
        NPartyProtocolSession session,
        Guid participantId,
        DateTime submittedUtc)
    {
        ArgumentNullException.ThrowIfNull(session);

        EnsureSessionNotClosed(session);

        if (session.SigningSession.Status == SessionStatus.Created)
            session.SigningSession.StartCommitmentsCollection();

        if (session.SigningSession.Status != SessionStatus.CommitmentsCollection)
            throw new InvalidOperationException("Commitments can only be published during the commitments phase.");

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
        NPartyProtocolSession session,
        Guid participantId,
        DateTime submittedUtc)
    {
        ArgumentNullException.ThrowIfNull(session);

        EnsureSessionNotClosed(session);

        if (!session.AllCommitmentsPublished)
            throw new InvalidOperationException("All commitments must be published before nonce reveal.");

        if (session.SigningSession.Status != SessionStatus.NonceRevealCollection)
            throw new InvalidOperationException("Nonce reveal is allowed only during the nonce reveal phase.");

        var participant = session.GetParticipant(participantId);

        var reveal = participant.RevealNonce(
            session.SessionId,
            submittedUtc,
            _commitmentService);

        if (session.AllNoncesRevealed)
        {
            var aggregateNoncePoint = SumNoncePoints(session.Participants.Values);
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
        NPartyProtocolSession session,
        Guid participantId,
        DateTime submittedUtc)
    {
        ArgumentNullException.ThrowIfNull(session);

        EnsureSessionNotClosed(session);

        if (!session.AllNoncesRevealed || session.Challenge is null)
            throw new InvalidOperationException("All nonce values must be revealed before partial signature submission.");

        if (session.SigningSession.Status != SessionStatus.PartialSignaturesCollection)
            throw new InvalidOperationException("Partial signatures can only be submitted during the partial signatures phase.");

        var participant = session.GetParticipant(participantId);

        var partialSignature = participant.CreatePartialSignature(
            session.SessionId,
            submittedUtc,
            _partialSignatureService,
            session.Challenge,
            session.ProtectionMode);

        if (session.AllPartialSignaturesSubmitted)
        {
            var aggregateScalar = SumPartialSignatureScalars(session.Participants.Values);

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

    private PointValue SumNoncePoints(IEnumerable<NPartyParticipantProtocolState> participants)
    {
        PointValue? sum = null;

        foreach (var participant in participants)
        {
            var point = participant.RevealRecord?.PublicNoncePoint
                ?? throw new InvalidOperationException("Participant nonce reveal is missing.");

            sum = sum is null ? point : _curveContext.AddPoints(sum, point);
        }

        return sum ?? throw new InvalidOperationException("Aggregate nonce point cannot be empty.");
    }

    private ScalarValue SumPartialSignatureScalars(IEnumerable<NPartyParticipantProtocolState> participants)
    {
        ScalarValue? sum = null;

        foreach (var participant in participants)
        {
            var scalar = participant.PartialSignatureRecord?.SignatureScalar.Value
                ?? throw new InvalidOperationException("Participant partial signature is missing.");

            sum = sum is null ? scalar : ScalarMath.AddMod(_curveContext, sum, scalar);
        }

        return sum ?? throw new InvalidOperationException("Aggregate signature scalar cannot be empty.");
    }

    private static void EnsureSessionNotClosed(NPartyProtocolSession session)
    {
        if (session.SigningSession.Status is SessionStatus.Completed or SessionStatus.Failed or SessionStatus.Cancelled)
            throw new InvalidOperationException("The signing session is already closed.");
    }
}