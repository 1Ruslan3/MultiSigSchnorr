namespace MultiSigSchnorr.Domain.ValueObjects;

public sealed class CommitmentValue : IEquatable<CommitmentValue>
{
    public byte[] Bytes { get; }

    public CommitmentValue(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        if (bytes.Length == 0)
            throw new ArgumentException("Commitment bytes cannot be empty.", nameof(bytes));

        Bytes = bytes.ToArray();
    }

    public static CommitmentValue FromHex(string hex) => new(Convert.FromHexString(hex));

    public string ToHex() => Convert.ToHexString(Bytes);

    public bool Equals(CommitmentValue? other)
    {
        if (other is null)
            return false;

        return Bytes.AsSpan().SequenceEqual(other.Bytes);
    }

    public override bool Equals(object? obj) => Equals(obj as CommitmentValue);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in Bytes)
            hash.Add(b);
        return hash.ToHashCode();
    }

    public override string ToString() => ToHex();
}