namespace MultiSigSchnorr.Domain.ValueObjects;

public sealed class PublicKeyValue : IEquatable<PublicKeyValue>
{
    public PointValue Point { get; }

    public PublicKeyValue(PointValue point)
    {
        Point = point ?? throw new ArgumentNullException(nameof(point));
    }

    public static PublicKeyValue FromHex(string hex) => new(PointValue.FromHex(hex));

    public string ToHex() => Point.ToHex();

    public bool Equals(PublicKeyValue? other)
    {
        if (other is null)
            return false;

        return Point.Equals(other.Point);
    }

    public override bool Equals(object? obj) => Equals(obj as PublicKeyValue);

    public override int GetHashCode() => Point.GetHashCode();

    public override string ToString() => ToHex();
}