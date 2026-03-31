using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Abstractions;

public interface IPointSerializer
{
    PointValue Deserialize(ReadOnlySpan<byte> bytes);
    byte[] Serialize(PointValue point);
}