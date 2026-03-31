using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Abstractions;

public interface IAggregateKeyService
{
    PublicKeyValue BuildAggregateKey(IReadOnlyList<PublicKeyValue> publicKeys);
}