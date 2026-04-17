using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Abstractions;

public interface IPublicKeyGenerationService
{
    PublicKeyValue DerivePublicKey(
        ScalarValue privateKey,
        SignatureProtectionMode protectionMode = SignatureProtectionMode.Baseline);
}