using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Serialization;

public sealed class ScalarSerializer : IScalarSerializer
{
    public ScalarValue Deserialize(ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty)
            throw new ArgumentException("Scalar bytes cannot be empty.", nameof(bytes));

        return new ScalarValue(bytes.ToArray());
    }

    public byte[] Serialize(ScalarValue scalar)
    {
        ArgumentNullException.ThrowIfNull(scalar);
        return scalar.Bytes.ToArray();
    }
}