using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Abstractions;

public interface IPublicKeyGenerationService
{
    PublicKeyValue DerivePublicKey(ScalarValue privateKey);
}