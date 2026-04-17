using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Crypto.Security;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Curves;

public sealed class PublicKeyGenerationService : IPublicKeyGenerationService
{
    private readonly ICurveContext _curveContext;
    private readonly ISecretScalarRandomizationService _secretScalarRandomizationService;

    public PublicKeyGenerationService(ICurveContext curveContext)
        : this(curveContext, new SecretScalarRandomizationService(curveContext))
    {
    }

    public PublicKeyGenerationService(
        ICurveContext curveContext,
        ISecretScalarRandomizationService secretScalarRandomizationService)
    {
        _curveContext = curveContext ?? throw new ArgumentNullException(nameof(curveContext));
        _secretScalarRandomizationService = secretScalarRandomizationService
            ?? throw new ArgumentNullException(nameof(secretScalarRandomizationService));
    }

    public PublicKeyValue DerivePublicKey(
        ScalarValue privateKey,
        SignatureProtectionMode protectionMode = SignatureProtectionMode.Baseline)
    {
        ArgumentNullException.ThrowIfNull(privateKey);

        if (privateKey.Bytes.All(static b => b == 0))
            throw new ArgumentException("Private key must be non-zero.", nameof(privateKey));

        return protectionMode switch
        {
            SignatureProtectionMode.Baseline => DeriveBaseline(privateKey),
            SignatureProtectionMode.RandomizedScalarProcessing => DeriveRandomized(privateKey),
            _ => throw new ArgumentOutOfRangeException(nameof(protectionMode), protectionMode, "Unsupported protection mode.")
        };
    }

    private PublicKeyValue DeriveBaseline(ScalarValue privateKey)
    {
        var publicPoint = _curveContext.MultiplyBasePoint(privateKey);
        return new PublicKeyValue(publicPoint);
    }

    private PublicKeyValue DeriveRandomized(ScalarValue privateKey)
    {
        var shares = _secretScalarRandomizationService.Split(privateKey);

        var leftPoint = _curveContext.MultiplyBasePoint(shares.FirstShare);
        var rightPoint = _curveContext.MultiplyBasePoint(shares.SecondShare);
        var publicPoint = _curveContext.AddPoints(leftPoint, rightPoint);

        return new PublicKeyValue(publicPoint);
    }
}