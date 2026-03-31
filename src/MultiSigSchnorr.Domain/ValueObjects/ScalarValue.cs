using System.Globalization;
using System.Numerics;

namespace MultiSigSchnorr.Domain.ValueObjects;

public sealed class ScalarValue : IEquatable<ScalarValue>
{
    public byte[] Bytes { get; }

    public ScalarValue(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        if (bytes.Length == 0)
            throw new ArgumentException("Scalar bytes cannot be empty.", nameof(bytes));

        Bytes = bytes.ToArray();
    }

    public static ScalarValue FromHex(string hex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hex);

        return new ScalarValue(Convert.FromHexString(NormalizeHex(hex)));
    }

    public BigInteger ToBigIntegerUnsignedBigEndian()
    {
        return new BigInteger(Bytes, isUnsigned: true, isBigEndian: true);
    }

    public string ToHex() => Convert.ToHexString(Bytes);

    public bool Equals(ScalarValue? other)
    {
        if (other is null)
            return false;

        return Bytes.AsSpan().SequenceEqual(other.Bytes);
    }

    public override bool Equals(object? obj) => Equals(obj as ScalarValue);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in Bytes)
            hash.Add(b);
        return hash.ToHashCode();
    }

    public override string ToString() => ToHex();

    private static string NormalizeHex(string hex)
    {
        return hex.StartsWith("0x", true, CultureInfo.InvariantCulture)
            ? hex[2..]
            : hex;
    }
}