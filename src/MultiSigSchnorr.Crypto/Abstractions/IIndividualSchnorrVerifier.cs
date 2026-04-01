using MultiSigSchnorr.Crypto.Schnorr;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Abstractions;

public interface IIndividualSchnorrVerifier
{
    bool Verify(
        SchnorrSignature signature,
        PublicKeyValue publicKey,
        MessageDigestValue messageDigest);
}