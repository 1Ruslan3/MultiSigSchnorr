namespace MultiSigSchnorr.Domain.ValueObjects;

public sealed class EpochStateHashValue : IEquatable<EpochStateHashValue>
{
    public byte[] Bytes { get; }

    public EpochStateHashValue(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        if (bytes.Length == 0)
            throw new ArgumentException("Epoch state hash cannot be empty.", nameof(bytes));

        Bytes = bytes.ToArray();
    }

    public string ToHex() => Convert.ToHexString(Bytes);

    public bool Equals(EpochStateHashValue? other)
    {
        if (other is null)
            return false;

        return Bytes.AsSpan().SequenceEqual(other.Bytes);
    }

    public override bool Equals(object? obj) => Equals(obj as EpochStateHashValue);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in Bytes)
            hash.Add(b);
        return hash.ToHashCode();
    }

    public override string ToString() => ToHex();
}