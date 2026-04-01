using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Schnorr;

public sealed class AggregateSignatureVerifier : IAggregateSignatureVerifier
{
    private readonly ICurveContext _curveContext;
    private readonly IChallengeService _challengeService;

    public AggregateSignatureVerifier(
        ICurveContext curveContext,
        IChallengeService challengeService)
    {
        _curveContext = curveContext ?? throw new ArgumentNullException(nameof(curveContext));
        _challengeService = challengeService ?? throw new ArgumentNullException(nameof(challengeService));
    }

    public bool Verify(
        AggregateSignature signature,
        PublicKeyValue aggregatePublicKey,
        MessageDigestValue messageDigest)
    {
        ArgumentNullException.ThrowIfNull(signature);
        ArgumentNullException.ThrowIfNull(aggregatePublicKey);
        ArgumentNullException.ThrowIfNull(messageDigest);

        if (!_curveContext.IsValidPoint(signature.AggregateNoncePoint))
            return false;

        if (!_curveContext.IsValidPoint(aggregatePublicKey.Point))
            return false;

        if (signature.SignatureScalar.Value.Bytes.All(static b => b == 0))
            return false;

        var challenge = _challengeService.ComputeChallenge(
            signature.AggregateNoncePoint,
            aggregatePublicKey,
            messageDigest);

        var left = _curveContext.MultiplyBasePoint(signature.SignatureScalar.Value);
        var right = _curveContext.AddPoints(
            signature.AggregateNoncePoint,
            _curveContext.MultiplyPoint(aggregatePublicKey.Point, challenge));

        return left.Equals(right);
    }
}