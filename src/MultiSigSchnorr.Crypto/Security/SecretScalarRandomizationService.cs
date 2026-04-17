using System.Security.Cryptography;
using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Security;

public sealed class SecretScalarRandomizationService : ISecretScalarRandomizationService
{
    private readonly ICurveContext _curveContext;

    public SecretScalarRandomizationService(ICurveContext curveContext)
    {
        _curveContext = curveContext ?? throw new ArgumentNullException(nameof(curveContext));
    }

    public SecretScalarSharePair Split(ScalarValue secretScalar)
    {
        ArgumentNullException.ThrowIfNull(secretScalar);

        if (IsZero(secretScalar))
            throw new ArgumentException("Secret scalar must be non-zero.", nameof(secretScalar));

        for (var attempt = 0; attempt < 32; attempt++)
        {
            var randomBytes = RandomNumberGenerator.GetBytes(_curveContext.ScalarSizeBytes);
            var firstShare = _curveContext.ReduceScalar(randomBytes);

            if (IsZero(firstShare))
                continue;

            var secondShare = ScalarMath.SubtractMod(_curveContext, secretScalar, firstShare);

            if (IsZero(secondShare))
                continue;

            return new SecretScalarSharePair(firstShare, secondShare);
        }

        var fallbackBytes = new byte[_curveContext.ScalarSizeBytes];
        fallbackBytes[^1] = 1;

        var fallbackFirst = new ScalarValue(fallbackBytes);
        var fallbackSecond = ScalarMath.SubtractMod(_curveContext, secretScalar, fallbackFirst);

        if (IsZero(fallbackSecond))
            throw new InvalidOperationException("Failed to generate a valid randomized scalar decomposition.");

        return new SecretScalarSharePair(fallbackFirst, fallbackSecond);
    }

    private static bool IsZero(ScalarValue scalar)
        => scalar.Bytes.All(static b => b == 0);
}