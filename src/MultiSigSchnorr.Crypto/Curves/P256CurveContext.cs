using System.Numerics;
using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Domain.Constants;
using MultiSigSchnorr.Domain.ValueObjects;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Math.EC;
using BcBigInteger = Org.BouncyCastle.Math.BigInteger;

namespace MultiSigSchnorr.Crypto.Curves;

public sealed class P256CurveContext : ICurveContext
{
    private readonly X9ECParameters _parameters;
    private readonly ECCurve _curve;
    private readonly ECPoint _generator;
    public string CurveName => CurveNames.P256;
    public int ScalarSizeBytes => 32;
    public BigInteger Order { get; }

    public P256CurveContext()
    {
        _parameters = SecNamedCurves.GetByName("secp256r1")
            ?? throw new InvalidOperationException("Curve secp256r1 was not found.");

        _curve = _parameters.Curve;
        _generator = _parameters.G;
        Order = ToSystemBigInteger(_parameters.N);
    }

    public bool IsValidPoint(PointValue point)
    {
        ArgumentNullException.ThrowIfNull(point);

        try
        {
            var decoded = DecodePoint(point);
            return !decoded.IsInfinity && decoded.IsValid();
        }
        catch
        {
            return false;
        }
    }

    public PointValue MultiplyBasePoint(ScalarValue scalar)
    {
        ArgumentNullException.ThrowIfNull(scalar);

        var k = ToBcBigInteger(scalar);
        if (k.SignValue == 0)
            throw new ArgumentException("Scalar must be non-zero.", nameof(scalar));

        var result = _generator.Multiply(k).Normalize();
        return new PointValue(result.GetEncoded(false));
    }

    public PointValue MultiplyPoint(PointValue point, ScalarValue scalar)
    {
        ArgumentNullException.ThrowIfNull(point);
        ArgumentNullException.ThrowIfNull(scalar);

        var p = DecodePoint(point);
        var k = ToBcBigInteger(scalar);

        var result = p.Multiply(k).Normalize();
        return new PointValue(result.GetEncoded(false));
    }

    public PointValue AddPoints(PointValue left, PointValue right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        var l = DecodePoint(left);
        var r = DecodePoint(right);

        var result = l.Add(r).Normalize();
        return new PointValue(result.GetEncoded(false));
    }

    public ScalarValue ReduceScalar(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
            throw new ArgumentException("Scalar source bytes cannot be empty.", nameof(data));

        var value = new BcBigInteger(1, data.ToArray()).Mod(_parameters.N);
        var systemValue = ToSystemBigInteger(value);

        return new ScalarValue(ToFixedLengthBigEndian(systemValue, ScalarSizeBytes));
    }

    private ECPoint DecodePoint(PointValue point)
    {
        var decoded = _curve.DecodePoint(point.Bytes).Normalize();

        if (!decoded.IsValid())
            throw new ArgumentException("Point is not valid for the selected curve.", nameof(point));

        return decoded;
    }

    private static BcBigInteger ToBcBigInteger(ScalarValue scalar)
    {
        return new BcBigInteger(1, scalar.Bytes);
    }

    private static BigInteger ToSystemBigInteger(BcBigInteger value)
    {
        return new BigInteger(value.ToByteArrayUnsigned(), isUnsigned: true, isBigEndian: true);
    }

    private static byte[] ToFixedLengthBigEndian(BigInteger value, int size)
    {
        var raw = value.ToByteArray(isUnsigned: true, isBigEndian: true);

        if (raw.Length == size)
            return raw;

        if (raw.Length > size)
            return raw[^size..];

        var result = new byte[size];
        Buffer.BlockCopy(raw, 0, result, size - raw.Length, raw.Length);
        return result;
    }
}