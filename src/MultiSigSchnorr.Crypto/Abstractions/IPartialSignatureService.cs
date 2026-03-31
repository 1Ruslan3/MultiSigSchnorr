using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Abstractions;

public interface IPartialSignatureService
{
    SignatureScalarValue CreatePartialSignature(
        ScalarValue nonce,
        ScalarValue privateKey,
        ScalarValue challenge,
        ScalarValue aggregationCoefficient);
}