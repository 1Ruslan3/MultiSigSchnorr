using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Crypto.Security;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Schnorr;

public sealed class IndividualSchnorrSigner : IIndividualSchnorrSigner
{
    private readonly ICurveContext _curveContext;
    private readonly INonceGenerator _nonceGenerator;
    private readonly IChallengeService _challengeService;

    public IndividualSchnorrSigner(
        ICurveContext curveContext,
        INonceGenerator nonceGenerator,
        IChallengeService challengeService)
    {
        _curveContext = curveContext ?? throw new ArgumentNullException(nameof(curveContext));
        _nonceGenerator = nonceGenerator ?? throw new ArgumentNullException(nameof(nonceGenerator));
        _challengeService = challengeService ?? throw new ArgumentNullException(nameof(challengeService));
    }

    public SchnorrSignature CreateSignature(
        ScalarValue privateKey,
        PublicKeyValue publicKey,
        MessageDigestValue messageDigest)
    {
        ArgumentNullException.ThrowIfNull(privateKey);
        ArgumentNullException.ThrowIfNull(publicKey);
        ArgumentNullException.ThrowIfNull(messageDigest);

        EnsureNonZero(privateKey, nameof(privateKey));

        var nonce = _nonceGenerator.GenerateNonce();
        var noncePoint = _nonceGenerator.CreatePublicNonce(nonce);

        var challenge = _challengeService.ComputeChallenge(
            noncePoint,
            publicKey,
            messageDigest);

        var cx = ScalarMath.MultiplyMod(_curveContext, challenge, privateKey);
        var s = ScalarMath.AddMod(_curveContext, nonce, cx);

        return new SchnorrSignature(
            noncePoint,
            new SignatureScalarValue(s));
    }

    private static void EnsureNonZero(ScalarValue scalar, string paramName)
    {
        if (scalar.Bytes.All(static b => b == 0))
            throw new ArgumentException("Scalar must be non-zero.", paramName);
    }
}