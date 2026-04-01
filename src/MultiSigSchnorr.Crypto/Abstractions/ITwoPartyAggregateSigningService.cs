using MultiSigSchnorr.Crypto.Aggregation;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Abstractions;

public interface ITwoPartyAggregateSigningService
{
    TwoPartyAggregateSigningResult Sign(
        ScalarValue firstPrivateKey,
        ScalarValue secondPrivateKey,
        MessageDigestValue messageDigest,
        Guid sessionId,
        DateTime createdUtc);
}