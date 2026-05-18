using MultiSigSchnorr.Domain.ValueObjects;
using Xunit;

namespace MultiSigSchnorr.Tests.CryptoVectors;

public sealed class ScalarValueVectorTests
{
    [Fact]
    public void FromHex_Should_Preserve_Valid_32Byte_Scalar_Vector()
    {
        const string scalarHex =
            "101112131415161718191A1B1C1D1E1F202122232425262728292A2B2C2D2E2F";

        var scalar = ScalarValue.FromHex(scalarHex);

        Assert.Equal(scalarHex, scalar.ToHex());
    }

    [Fact]
    public void FromHex_Should_Normalize_Lowercase_Hex_To_Uppercase()
    {
        const string scalarHex =
            "101112131415161718191a1b1c1d1e1f202122232425262728292a2b2c2d2e2f";

        var scalar = ScalarValue.FromHex(scalarHex);

        Assert.Equal(
            "101112131415161718191A1B1C1D1E1F202122232425262728292A2B2C2D2E2F",
            scalar.ToHex());
    }

    [Fact]
    public void FromHex_Should_Reject_Empty_Value()
    {
        Assert.Throws<ArgumentException>(() => ScalarValue.FromHex(string.Empty));
    }
}