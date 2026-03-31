using System.Globalization;

namespace MultiSigSchnorr.Domain.ValueObjects;

public sealed class PointValue : IEquatable<PointValue>
{
    public byte[] Bytes { get; }

    public PointValue(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        if (bytes.Length == 0)
            throw new ArgumentException("Point bytes cannot be empty.", nameof(bytes));

        Bytes = bytes.ToArray();
    }

    public static PointValue FromHex(string hex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hex);

        return new PointValue(Convert.FromHexString(NormalizeHex(hex)));
    }

    public string ToHex() => Convert.ToHexString(Bytes);

    public bool Equals(PointValue? other)
    {
        if (other is null)
            return false;

        return Bytes.AsSpan().SequenceEqual(other.Bytes);
    }

    public override bool Equals(object? obj) => Equals(obj as PointValue);

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