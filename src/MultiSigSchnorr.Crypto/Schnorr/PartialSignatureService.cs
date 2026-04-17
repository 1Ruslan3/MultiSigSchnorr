using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Crypto.Security;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Schnorr;

public sealed class PartialSignatureService : IPartialSignatureService
{
    private readonly ICurveContext _curveContext;
    private readonly ISecretScalarRandomizationService _secretScalarRandomizationService;

    public PartialSignatureService(ICurveContext curveContext)
        : this(curveContext, new SecretScalarRandomizationService(curveContext))
    {
    }

    public PartialSignatureService(
        ICurveContext curveContext,
        ISecretScalarRandomizationService secretScalarRandomizationService)
    {
        _curveContext = curveContext ?? throw new ArgumentNullException(nameof(curveContext));
        _secretScalarRandomizationService = secretScalarRandomizationService
            ?? throw new ArgumentNullException(nameof(secretScalarRandomizationService));
    }

    public SignatureScalarValue CreatePartialSignature(
        ScalarValue nonce,
        ScalarValue privateKey,
        ScalarValue challenge,
        ScalarValue aggregationCoefficient,
        SignatureProtectionMode protectionMode = SignatureProtectionMode.Baseline)
    {
        ArgumentNullException.ThrowIfNull(nonce);
        ArgumentNullException.ThrowIfNull(privateKey);
        ArgumentNullException.ThrowIfNull(challenge);
        ArgumentNullException.ThrowIfNull(aggregationCoefficient);

        EnsureNonZero(nonce, nameof(nonce));
        EnsureNonZero(privateKey, nameof(privateKey));

        return protectionMode switch
        {
            SignatureProtectionMode.Baseline => CreateBaseline(
                nonce,
                privateKey,
                challenge,
                aggregationCoefficient),

            SignatureProtectionMode.RandomizedScalarProcessing => CreateRandomized(
                nonce,
                privateKey,
                challenge,
                aggregationCoefficient),

            _ => throw new ArgumentOutOfRangeException(nameof(protectionMode), protectionMode, "Unsupported protection mode.")
        };
    }

    private SignatureScalarValue CreateBaseline(
        ScalarValue nonce,
        ScalarValue privateKey,
        ScalarValue challenge,
        ScalarValue aggregationCoefficient)
    {
        var weightedChallenge = ScalarMath.MultiplyMod(
            _curveContext,
            challenge,
            aggregationCoefficient);

        var secretContribution = ScalarMath.MultiplyMod(
            _curveContext,
            weightedChallenge,
            privateKey);

        var signatureScalar = ScalarMath.SubtractMod(
            _curveContext,
            nonce,
            secretContribution);

        return new SignatureScalarValue(signatureScalar);
    }

    private SignatureScalarValue CreateRandomized(
        ScalarValue nonce,
        ScalarValue privateKey,
        ScalarValue challenge,
        ScalarValue aggregationCoefficient)
    {
        var weightedChallenge = ScalarMath.MultiplyMod(
            _curveContext,
            challenge,
            aggregationCoefficient);

        var shares = _secretScalarRandomizationService.Split(privateKey);

        var firstContribution = ScalarMath.MultiplyMod(
            _curveContext,
            weightedChallenge,
            shares.FirstShare);

        var secondContribution = ScalarMath.MultiplyMod(
            _curveContext,
            weightedChallenge,
            shares.SecondShare);

        var secretContribution = ScalarMath.AddMod(
            _curveContext,
            firstContribution,
            secondContribution);

        var signatureScalar = ScalarMath.SubtractMod(
            _curveContext,
            nonce,
            secretContribution);

        return new SignatureScalarValue(signatureScalar);
    }

    private static void EnsureNonZero(ScalarValue scalar, string paramName)
    {
        if (scalar.Bytes.All(static b => b == 0))
            throw new ArgumentException("Scalar must be non-zero.", paramName);
    }
}