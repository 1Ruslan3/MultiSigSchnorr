using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Aggregation;

public sealed class AggregateKeyComputationResult
{
    private readonly IReadOnlyList<AggregateKeyEntry> _entries;

    public IReadOnlyList<AggregateKeyEntry> Entries => _entries;
    public PublicKeyValue AggregatePublicKey { get; }

    public AggregateKeyComputationResult(
        IReadOnlyList<AggregateKeyEntry> entries,
        PublicKeyValue aggregatePublicKey)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(aggregatePublicKey);

        if (entries.Count == 0)
            throw new ArgumentException("At least one aggregate key entry is required.", nameof(entries));

        _entries = entries;
        AggregatePublicKey = aggregatePublicKey;
    }

    public ScalarValue GetCoefficient(PublicKeyValue publicKey)
    {
        ArgumentNullException.ThrowIfNull(publicKey);

        var entry = _entries.FirstOrDefault(x => x.PublicKey.Equals(publicKey));
        if (entry is null)
            throw new InvalidOperationException("Public key is not present in aggregate key computation result.");

        return entry.Coefficient;
    }
}