using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Abstractions;

public interface INonceGenerator
{
    ScalarValue GenerateNonce();
    PointValue CreatePublicNonce(ScalarValue nonce);
}