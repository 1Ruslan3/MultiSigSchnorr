using MultiSigSchnorr.Crypto.Curves;
using MultiSigSchnorr.Crypto.Schnorr;
using MultiSigSchnorr.Crypto.Security;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Tests.Unit.Crypto.Schnorr;

public sealed class PartialSignatureServiceProtectionTests
{
    [Fact]
    public void CreatePartialSignature_Should_Be_Equivalent_To_Baseline_Formula()
    {
        var curveContext = new P256CurveContext();
        var service = new PartialSignatureService(curveContext);

        var nonce = ScalarValue.FromHex(
            "7172737475767778797A7B7C7D7E7F808182838485868788898A8B8C8D8E8F90");

        var privateKey = ScalarValue.FromHex(
            "101112131415161718191A1B1C1D1E1F202122232425262728292A2B2C2D2E2F");

        var challenge = ScalarValue.FromHex(
            "0BC578F7D4673385412294724AF7ED73D57F200EC57E43B44700AF16C411590A");

        var aggregationCoefficient = ScalarValue.FromHex(
            "DA072C9A813DD48348D0AC9BF8D76FA773DC5868C3962B631EFA19E010D839AE");

        var actual = service.CreatePartialSignature(
            nonce,
            privateKey,
            challenge,
            aggregationCoefficient);

        var weightedChallenge = ScalarMath.MultiplyMod(
            curveContext,
            challenge,
            aggregationCoefficient);

        var baselineContribution = ScalarMath.MultiplyMod(
            curveContext,
            weightedChallenge,
            privateKey);

        var baselineScalar = ScalarMath.SubtractMod(
            curveContext,
            nonce,
            baselineContribution);

        var expected = new SignatureScalarValue(baselineScalar);

        Assert.Equal(expected.ToHex(), actual.ToHex());
    }
}