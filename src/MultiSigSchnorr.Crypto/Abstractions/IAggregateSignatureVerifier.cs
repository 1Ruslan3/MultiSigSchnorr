using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Abstractions;

public interface IAggregateSignatureVerifier
{
    bool Verify(
        AggregateSignature signature,
        PublicKeyValue aggregatePublicKey,
        MessageDigestValue messageDigest);
}