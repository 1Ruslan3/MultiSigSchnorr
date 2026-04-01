using MultiSigSchnorr.Crypto.Aggregation;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Abstractions;

public interface IAggregateKeyService
{
    PublicKeyValue BuildAggregateKey(IReadOnlyList<PublicKeyValue> publicKeys);
    AggregateKeyComputationResult Compute(IReadOnlyList<PublicKeyValue> publicKeys);
}