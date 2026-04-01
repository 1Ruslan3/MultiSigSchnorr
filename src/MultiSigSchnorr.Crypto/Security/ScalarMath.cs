using System.Numerics;
using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Security;

public static class ScalarMath
{
    public static ScalarValue AddMod(ICurveContext curveContext, ScalarValue left, ScalarValue right)
    {
        ArgumentNullException.ThrowIfNull(curveContext);
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var l = ToBigInteger(left);
        var r = ToBigInteger(right);
        var result = Mod(l + r, curveContext.Order);

        return ToScalar(curveContext, result);
    }

    public static ScalarValue MultiplyMod(ICurveContext curveContext, ScalarValue left, ScalarValue right)
    {
        ArgumentNullException.ThrowIfNull(curveContext);
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var l = ToBigInteger(left);
        var r = ToBigInteger(right);
        var result = Mod(l * r, curveContext.Order);

        return ToScalar(curveContext, result);
    }

    public static ScalarValue ToScalar(ICurveContext curveContext, BigInteger value)
    {
        ArgumentNullException.ThrowIfNull(curveContext);

        var reduced = Mod(value, curveContext.Order);
        var bytes = reduced.ToByteArray(isUnsigned: true, isBigEndian: true);

        if (bytes.Length == curveContext.ScalarSizeBytes)
            return new ScalarValue(bytes);

        if (bytes.Length > curveContext.ScalarSizeBytes)
            return new ScalarValue(bytes[^curveContext.ScalarSizeBytes..]);

        var result = new byte[curveContext.ScalarSizeBytes];
        Buffer.BlockCopy(bytes, 0, result, curveContext.ScalarSizeBytes - bytes.Length, bytes.Length);

        return new ScalarValue(result);
    }

    public static BigInteger ToBigInteger(ScalarValue scalar)
    {
        ArgumentNullException.ThrowIfNull(scalar);
        return new BigInteger(scalar.Bytes, isUnsigned: true, isBigEndian: true);
    }

    private static BigInteger Mod(BigInteger value, BigInteger modulus)
    {
        var result = value % modulus;
        return result.Sign < 0 ? result + modulus : result;
    }
}