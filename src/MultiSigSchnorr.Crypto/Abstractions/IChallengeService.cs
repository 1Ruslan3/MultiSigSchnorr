using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Abstractions;

public interface IChallengeService
{
    ScalarValue ComputeChallenge(
        PointValue aggregateNoncePoint,
        PublicKeyValue aggregatePublicKey,
        MessageDigestValue messageDigest);
}