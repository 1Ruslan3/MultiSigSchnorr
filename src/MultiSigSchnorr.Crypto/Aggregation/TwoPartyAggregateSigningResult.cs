using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Aggregation;

public sealed class TwoPartyAggregateSigningResult
{
    public PublicKeyValue FirstPublicKey { get; }
    public PublicKeyValue SecondPublicKey { get; }
    public PublicKeyValue AggregatePublicKey { get; }

    public SignatureScalarValue FirstPartialSignature { get; }
    public SignatureScalarValue SecondPartialSignature { get; }

    public AggregateSignature AggregateSignature { get; }

    public TwoPartyAggregateSigningResult(
        PublicKeyValue firstPublicKey,
        PublicKeyValue secondPublicKey,
        PublicKeyValue aggregatePublicKey,
        SignatureScalarValue firstPartialSignature,
        SignatureScalarValue secondPartialSignature,
        AggregateSignature aggregateSignature)
    {
        FirstPublicKey = firstPublicKey ?? throw new ArgumentNullException(nameof(firstPublicKey));
        SecondPublicKey = secondPublicKey ?? throw new ArgumentNullException(nameof(secondPublicKey));
        AggregatePublicKey = aggregatePublicKey ?? throw new ArgumentNullException(nameof(aggregatePublicKey));
        FirstPartialSignature = firstPartialSignature ?? throw new ArgumentNullException(nameof(firstPartialSignature));
        SecondPartialSignature = secondPartialSignature ?? throw new ArgumentNullException(nameof(secondPartialSignature));
        AggregateSignature = aggregateSignature ?? throw new ArgumentNullException(nameof(aggregateSignature));
    }
}