namespace MultiSigSchnorr.Domain.ValueObjects;

public sealed class SignatureScalarValue : IEquatable<SignatureScalarValue>
{
    public ScalarValue Value { get; }

    public SignatureScalarValue(ScalarValue value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static SignatureScalarValue FromHex(string hex) => new(ScalarValue.FromHex(hex));

    public string ToHex() => Value.ToHex();

    public bool Equals(SignatureScalarValue? other)
    {
        if (other is null)
            return false;

        return Value.Equals(other.Value);
    }

    public override bool Equals(object? obj) => Equals(obj as SignatureScalarValue);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => ToHex();
}