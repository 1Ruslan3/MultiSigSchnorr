using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Serialization;

public sealed class PointSerializer : IPointSerializer
{
    public PointValue Deserialize(ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty)
            throw new ArgumentException("Point bytes cannot be empty.", nameof(bytes));

        return new PointValue(bytes.ToArray());
    }

    public byte[] Serialize(PointValue point)
    {
        ArgumentNullException.ThrowIfNull(point);
        return point.Bytes.ToArray();
    }
}