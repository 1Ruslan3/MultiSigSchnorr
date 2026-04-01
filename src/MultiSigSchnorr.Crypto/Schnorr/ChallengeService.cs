using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Domain.Constants;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Schnorr;

public sealed class ChallengeService : IChallengeService
{
    private readonly IHashToScalarService _hashToScalarService;

    public ChallengeService(IHashToScalarService hashToScalarService)
    {
        _hashToScalarService = hashToScalarService ?? throw new ArgumentNullException(nameof(hashToScalarService));
    }

    public ScalarValue ComputeChallenge(
        PointValue aggregateNoncePoint,
        PublicKeyValue aggregatePublicKey,
        MessageDigestValue messageDigest)
    {
        ArgumentNullException.ThrowIfNull(aggregateNoncePoint);
        ArgumentNullException.ThrowIfNull(aggregatePublicKey);
        ArgumentNullException.ThrowIfNull(messageDigest);

        return _hashToScalarService.HashToScalar(
            DomainSeparationTags.Challenge,
            aggregateNoncePoint.Bytes,
            aggregatePublicKey.Point.Bytes,
            messageDigest.Bytes);
    }
}