using MultiSigSchnorr.Crypto.Curves;
using MultiSigSchnorr.Crypto.Security;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Tests.Unit.Crypto.Security;

public sealed class SecretScalarRandomizationServiceTests
{
    [Fact]
    public void Split_Should_Return_Two_NonZero_Shares_Whose_Sum_Equals_Original_Scalar()
    {
        var curveContext = new P256CurveContext();
        var service = new SecretScalarRandomizationService(curveContext);

        var secretScalar = ScalarValue.FromHex(
            "101112131415161718191A1B1C1D1E1F202122232425262728292A2B2C2D2E2F");

        var shares = service.Split(secretScalar);

        Assert.False(IsZero(shares.FirstShare));
        Assert.False(IsZero(shares.SecondShare));

        var reconstructed = ScalarMath.AddMod(
            curveContext,
            shares.FirstShare,
            shares.SecondShare);

        Assert.Equal(secretScalar.ToHex(), reconstructed.ToHex());
    }

    [Fact]
    public void Split_With_Zero_Scalar_Should_Throw()
    {
        var curveContext = new P256CurveContext();
        var service = new SecretScalarRandomizationService(curveContext);

        var zeroBytes = new byte[curveContext.ScalarSizeBytes];
        var zeroScalar = new ScalarValue(zeroBytes);

        var ex = Assert.Throws<ArgumentException>(() => service.Split(zeroScalar));
        Assert.Contains("Secret scalar must be non-zero", ex.Message, StringComparison.Ordinal);
    }

    private static bool IsZero(ScalarValue scalar)
        => scalar.Bytes.All(static b => b == 0);
}