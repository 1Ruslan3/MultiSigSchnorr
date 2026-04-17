using MultiSigSchnorr.Crypto.Curves;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Tests.Unit.Crypto.Curves;

public sealed class PublicKeyGenerationServiceProtectionTests
{
    [Fact]
    public void DerivePublicKey_Should_Be_Equivalent_To_Direct_BasePoint_Multiplication()
    {
        var curveContext = new P256CurveContext();
        var service = new PublicKeyGenerationService(curveContext);

        var privateKey = ScalarValue.FromHex(
            "3132333435363738393A3B3C3D3E3F404142434445464748494A4B4C4D4E4F50");

        var expected = new PublicKeyValue(curveContext.MultiplyBasePoint(privateKey));
        var actual = service.DerivePublicKey(privateKey);

        Assert.Equal(expected.ToHex(), actual.ToHex());
    }
}