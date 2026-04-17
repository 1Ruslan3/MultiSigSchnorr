using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Crypto.Security;

public sealed class SecretScalarSharePair
{
    public ScalarValue FirstShare { get; }
    public ScalarValue SecondShare { get; }

    public SecretScalarSharePair(ScalarValue firstShare, ScalarValue secondShare)
    {
        FirstShare = firstShare ?? throw new ArgumentNullException(nameof(firstShare));
        SecondShare = secondShare ?? throw new ArgumentNullException(nameof(secondShare));
    }
}