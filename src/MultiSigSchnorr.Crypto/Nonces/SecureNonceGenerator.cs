using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Domain.Abstractions;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Nonces;

public sealed class SecureNonceGenerator : INonceGenerator
{
    private readonly ICurveContext _curveContext;
    private readonly IRandomSource _randomSource;

    public SecureNonceGenerator(ICurveContext curveContext, IRandomSource randomSource)
    {
        _curveContext = curveContext ?? throw new ArgumentNullException(nameof(curveContext));
        _randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
    }

    public ScalarValue GenerateNonce()
    {
        var buffer = new byte[_curveContext.ScalarSizeBytes + 16];

        while (true)
        {
            _randomSource.Fill(buffer);
            var nonce = _curveContext.ReduceScalar(buffer);

            if (!IsZero(nonce))
                return nonce;
        }
    }

    public PointValue CreatePublicNonce(ScalarValue nonce)
    {
        ArgumentNullException.ThrowIfNull(nonce);
        return _curveContext.MultiplyBasePoint(nonce);
    }

    private static bool IsZero(ScalarValue scalar)
    {
        return scalar.Bytes.All(static b => b == 0);
    }
}