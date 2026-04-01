using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Crypto.Security;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Aggregation;

public sealed class TwoPartyAggregateSigningService : ITwoPartyAggregateSigningService
{
    private readonly IPublicKeyGenerationService _publicKeyGenerationService;
    private readonly IAggregateKeyService _aggregateKeyService;
    private readonly INonceGenerator _nonceGenerator;
    private readonly ICurveContext _curveContext;
    private readonly IChallengeService _challengeService;
    private readonly IPartialSignatureService _partialSignatureService;

    public TwoPartyAggregateSigningService(
        IPublicKeyGenerationService publicKeyGenerationService,
        IAggregateKeyService aggregateKeyService,
        INonceGenerator nonceGenerator,
        ICurveContext curveContext,
        IChallengeService challengeService,
        IPartialSignatureService partialSignatureService)
    {
        _publicKeyGenerationService = publicKeyGenerationService ?? throw new ArgumentNullException(nameof(publicKeyGenerationService));
        _aggregateKeyService = aggregateKeyService ?? throw new ArgumentNullException(nameof(aggregateKeyService));
        _nonceGenerator = nonceGenerator ?? throw new ArgumentNullException(nameof(nonceGenerator));
        _curveContext = curveContext ?? throw new ArgumentNullException(nameof(curveContext));
        _challengeService = challengeService ?? throw new ArgumentNullException(nameof(challengeService));
        _partialSignatureService = partialSignatureService ?? throw new ArgumentNullException(nameof(partialSignatureService));
    }

    public TwoPartyAggregateSigningResult Sign(
        ScalarValue firstPrivateKey,
        ScalarValue secondPrivateKey,
        MessageDigestValue messageDigest,
        Guid sessionId,
        DateTime createdUtc)
    {
        ArgumentNullException.ThrowIfNull(firstPrivateKey);
        ArgumentNullException.ThrowIfNull(secondPrivateKey);
        ArgumentNullException.ThrowIfNull(messageDigest);

        if (sessionId == Guid.Empty)
            throw new ArgumentException("Session id cannot be empty.", nameof(sessionId));

        var firstPublicKey = _publicKeyGenerationService.DerivePublicKey(firstPrivateKey);
        var secondPublicKey = _publicKeyGenerationService.DerivePublicKey(secondPrivateKey);

        var aggregateKeyResult = _aggregateKeyService.Compute(
            new[] { firstPublicKey, secondPublicKey });

        var k1 = _nonceGenerator.GenerateNonce();
        var k2 = _nonceGenerator.GenerateNonce();

        var r1 = _nonceGenerator.CreatePublicNonce(k1);
        var r2 = _nonceGenerator.CreatePublicNonce(k2);

        var aggregateNoncePoint = _curveContext.AddPoints(r1, r2);

        var challenge = _challengeService.ComputeChallenge(
            aggregateNoncePoint,
            aggregateKeyResult.AggregatePublicKey,
            messageDigest);

        var a1 = aggregateKeyResult.GetCoefficient(firstPublicKey);
        var a2 = aggregateKeyResult.GetCoefficient(secondPublicKey);

        var s1 = _partialSignatureService.CreatePartialSignature(k1, firstPrivateKey, challenge, a1);
        var s2 = _partialSignatureService.CreatePartialSignature(k2, secondPrivateKey, challenge, a2);

        var aggregateScalar = ScalarMath.AddMod(_curveContext, s1.Value, s2.Value);

        var aggregateSignature = new AggregateSignature(
            Guid.NewGuid(),
            sessionId,
            aggregateNoncePoint,
            new SignatureScalarValue(aggregateScalar),
            createdUtc);

        return new TwoPartyAggregateSigningResult(
            firstPublicKey,
            secondPublicKey,
            aggregateKeyResult.AggregatePublicKey,
            s1,
            s2,
            aggregateSignature);
    }
}