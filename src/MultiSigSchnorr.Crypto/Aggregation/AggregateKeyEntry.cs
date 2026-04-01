using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Aggregation;

public sealed class AggregateKeyEntry
{
    public PublicKeyValue PublicKey { get; }
    public ScalarValue Coefficient { get; }

    public AggregateKeyEntry(PublicKeyValue publicKey, ScalarValue coefficient)
    {
        PublicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
        Coefficient = coefficient ?? throw new ArgumentNullException(nameof(coefficient));
    }
}