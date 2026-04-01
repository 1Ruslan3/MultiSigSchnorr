using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Curves;

public sealed class PublicKeyGenerationService : IPublicKeyGenerationService
{
    private readonly ICurveContext _curveContext;

    public PublicKeyGenerationService(ICurveContext curveContext)
    {
        _curveContext = curveContext ?? throw new ArgumentNullException(nameof(curveContext));
    }

    public PublicKeyValue DerivePublicKey(ScalarValue privateKey)
    {
        ArgumentNullException.ThrowIfNull(privateKey);

        if (privateKey.Bytes.All(static b => b == 0))
            throw new ArgumentException("Private key must be non-zero.", nameof(privateKey));

        var point = _curveContext.MultiplyBasePoint(privateKey);
        return new PublicKeyValue(point);
    }
}