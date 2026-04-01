using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Crypto.Security;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Schnorr;

public sealed class PartialSignatureService : IPartialSignatureService
{
    private readonly ICurveContext _curveContext;

    public PartialSignatureService(ICurveContext curveContext)
    {
        _curveContext = curveContext ?? throw new ArgumentNullException(nameof(curveContext));
    }

    public SignatureScalarValue CreatePartialSignature(
        ScalarValue nonce,
        ScalarValue privateKey,
        ScalarValue challenge,
        ScalarValue aggregationCoefficient)
    {
        ArgumentNullException.ThrowIfNull(nonce);
        ArgumentNullException.ThrowIfNull(privateKey);
        ArgumentNullException.ThrowIfNull(challenge);
        ArgumentNullException.ThrowIfNull(aggregationCoefficient);

        var ax = ScalarMath.MultiplyMod(_curveContext, aggregationCoefficient, privateKey);
        var cax = ScalarMath.MultiplyMod(_curveContext, challenge, ax);
        var s = ScalarMath.SubtractMod(_curveContext, nonce, cax);

        return new SignatureScalarValue(s);
    }
}