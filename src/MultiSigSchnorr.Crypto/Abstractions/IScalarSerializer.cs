using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Abstractions;

public interface IScalarSerializer
{
    ScalarValue Deserialize(ReadOnlySpan<byte> bytes);
    byte[] Serialize(ScalarValue scalar);
}