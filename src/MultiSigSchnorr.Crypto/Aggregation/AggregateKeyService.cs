using MultiSigSchnorr.Crypto.Abstractions;
using MultiSigSchnorr.Domain.Constants;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Aggregation;

public sealed class AggregateKeyService : IAggregateKeyService
{
    private readonly ICurveContext _curveContext;
    private readonly IHashToScalarService _hashToScalarService;

    public AggregateKeyService(
        ICurveContext curveContext,
        IHashToScalarService hashToScalarService)
    {
        _curveContext = curveContext ?? throw new ArgumentNullException(nameof(curveContext));
        _hashToScalarService = hashToScalarService ?? throw new ArgumentNullException(nameof(hashToScalarService));
    }

    public PublicKeyValue BuildAggregateKey(IReadOnlyList<PublicKeyValue> publicKeys)
    {
        return Compute(publicKeys).AggregatePublicKey;
    }

    public AggregateKeyComputationResult Compute(IReadOnlyList<PublicKeyValue> publicKeys)
    {
        ArgumentNullException.ThrowIfNull(publicKeys);

        if (publicKeys.Count == 0)
            throw new ArgumentException("At least one public key is required.", nameof(publicKeys));

        var orderedKeys = Canonicalize(publicKeys);

        PointValue? aggregatePoint = null;
        var entries = new List<AggregateKeyEntry>(orderedKeys.Count);

        foreach (var publicKey in orderedKeys)
        {
            var coefficient = ComputeCoefficient(orderedKeys, publicKey);

            if (coefficient.Bytes.All(static b => b == 0))
                throw new InvalidOperationException("Aggregation coefficient must not be zero.");

            var weightedPoint = _curveContext.MultiplyPoint(publicKey.Point, coefficient);

            aggregatePoint = aggregatePoint is null
                ? weightedPoint
                : _curveContext.AddPoints(aggregatePoint, weightedPoint);

            entries.Add(new AggregateKeyEntry(publicKey, coefficient));
        }

        return new AggregateKeyComputationResult(
            entries,
            new PublicKeyValue(aggregatePoint!));
    }

    private ScalarValue ComputeCoefficient(
        IReadOnlyList<PublicKeyValue> orderedKeys,
        PublicKeyValue publicKey)
    {
        var parts = new byte[orderedKeys.Count + 1][];

        for (var i = 0; i < orderedKeys.Count; i++)
            parts[i] = orderedKeys[i].Point.Bytes;

        parts[^1] = publicKey.Point.Bytes;

        return _hashToScalarService.HashToScalar(DomainSeparationTags.AggregateKey, parts);
    }

    private static IReadOnlyList<PublicKeyValue> Canonicalize(IReadOnlyList<PublicKeyValue> publicKeys)
    {
        var ordered = publicKeys
            .OrderBy(x => x.ToHex(), StringComparer.Ordinal)
            .ToList();

        for (var i = 1; i < ordered.Count; i++)
        {
            if (ordered[i].Equals(ordered[i - 1]))
                throw new InvalidOperationException("Duplicate public keys are not allowed in aggregate key computation.");
        }

        return ordered;
    }
}