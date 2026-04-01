using MultiSigSchnorr.Crypto.Schnorr;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Abstractions;

public interface IIndividualSchnorrSigner
{
    SchnorrSignature CreateSignature(
        ScalarValue privateKey,
        PublicKeyValue publicKey,
        MessageDigestValue messageDigest);
}