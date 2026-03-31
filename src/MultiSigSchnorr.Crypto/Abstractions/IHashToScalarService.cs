using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Abstractions;

public interface IHashToScalarService
{
    ScalarValue HashToScalar(string domainTag, params byte[][] parts);
}