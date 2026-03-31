namespace MultiSigSchnorr.Domain.ValueObjects;

public sealed class MessageDigestValue : IEquatable<MessageDigestValue>
{
    public byte[] Bytes { get; }

    public MessageDigestValue(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        if (bytes.Length == 0)
            throw new ArgumentException("Message digest cannot be empty.", nameof(bytes));

        Bytes = bytes.ToArray();
    }

    public string ToHex() => Convert.ToHexString(Bytes);

    public bool Equals(MessageDigestValue? other)
    {
        if (other is null)
            return false;

        return Bytes.AsSpan().SequenceEqual(other.Bytes);
    }

    public override bool Equals(object? obj) => Equals(obj as MessageDigestValue);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var b in Bytes)
            hash.Add(b);
        return hash.ToHashCode();
    }

    public override string ToString() => ToHex();
}