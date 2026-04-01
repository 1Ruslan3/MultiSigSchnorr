using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Schnorr;

public sealed class SchnorrSignature : IEquatable<SchnorrSignature>
{
    public PointValue NoncePoint { get; }
    public SignatureScalarValue SignatureScalar { get; }

    public SchnorrSignature(PointValue noncePoint, SignatureScalarValue signatureScalar)
    {
        NoncePoint = noncePoint ?? throw new ArgumentNullException(nameof(noncePoint));
        SignatureScalar = signatureScalar ?? throw new ArgumentNullException(nameof(signatureScalar));
    }

    public bool Equals(SchnorrSignature? other)
    {
        if (other is null)
            return false;

        return NoncePoint.Equals(other.NoncePoint)
               && SignatureScalar.Equals(other.SignatureScalar);
    }

    public override bool Equals(object? obj) => Equals(obj as SchnorrSignature);

    public override int GetHashCode() => HashCode.Combine(NoncePoint, SignatureScalar);

    public override string ToString() => $"{NoncePoint.ToHex()}:{SignatureScalar.ToHex()}";
}