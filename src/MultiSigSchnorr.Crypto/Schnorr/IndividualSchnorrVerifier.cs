using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Schnorr;

public sealed class IndividualSchnorrVerifier : IIndividualSchnorrVerifier
{
    private readonly ICurveContext _curveContext;
    private readonly IChallengeService _challengeService;

    public IndividualSchnorrVerifier(
        ICurveContext curveContext,
        IChallengeService challengeService)
    {
        _curveContext = curveContext ?? throw new ArgumentNullException(nameof(curveContext));
        _challengeService = challengeService ?? throw new ArgumentNullException(nameof(challengeService));
    }

    public bool Verify(
        SchnorrSignature signature,
        PublicKeyValue publicKey,
        MessageDigestValue messageDigest)
    {
        ArgumentNullException.ThrowIfNull(signature);
        ArgumentNullException.ThrowIfNull(publicKey);
        ArgumentNullException.ThrowIfNull(messageDigest);

        if (!_curveContext.IsValidPoint(signature.NoncePoint))
            return false;

        if (!_curveContext.IsValidPoint(publicKey.Point))
            return false;

        if (signature.SignatureScalar.Value.Bytes.All(static b => b == 0))
            return false;

        var challenge = _challengeService.ComputeChallenge(
            signature.NoncePoint,
            publicKey,
            messageDigest);

        var left = _curveContext.MultiplyBasePoint(signature.SignatureScalar.Value);
        var challengePublicKey = _curveContext.MultiplyPoint(publicKey.Point, challenge);
        var right = _curveContext.AddPoints(signature.NoncePoint, challengePublicKey);

        return left.Equals(right);
    }
}