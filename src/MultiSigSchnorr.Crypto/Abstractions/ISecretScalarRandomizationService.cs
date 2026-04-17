using MultiSigSchnorr.Crypto.Security;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Abstractions;

public interface ISecretScalarRandomizationService
{
    SecretScalarSharePair Split(ScalarValue secretScalar);
}