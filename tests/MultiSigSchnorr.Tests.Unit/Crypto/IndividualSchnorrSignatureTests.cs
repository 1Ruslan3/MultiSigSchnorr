using MultiSigSchnorr.Crypto.Curves;
using MultiSigSchnorr.Crypto.Hashing;
using MultiSigSchnorr.Crypto.Nonces;
using MultiSigSchnorr.Crypto.Schnorr;
using MultiSigSchnorr.Crypto.Security;
using MultiSigSchnorr.Domain.ValueObjects;
using Xunit;

namespace MultiSigSchnorr.Tests.Unit.Crypto;

public sealed class IndividualSchnorrSignatureTests
{
    [Fact]
    public void Sign_And_Verify_Should_Return_True()
    {
        var curve = new P256CurveContext();
        var randomSource = new SystemRandomSource();
        var nonceGenerator = new SecureNonceGenerator(curve, randomSource);
        var hashService = new Sha256HashService();
        var hashToScalar = new HashToScalarService(curve, hashService);
        var challengeService = new ChallengeService(hashToScalar);
        var messageDigestService = new MessageDigestService();

        var privateKey = curve.ReduceScalar(new byte[]
        {
            0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88,
            0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0x01, 0x02,
            0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A,
            0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12
        });

        var publicKey = new PublicKeyValue(curve.MultiplyBasePoint(privateKey));
        var digest = messageDigestService.DigestUtf8("test-message");

        var signer = new IndividualSchnorrSigner(curve, nonceGenerator, challengeService);
        var verifier = new IndividualSchnorrVerifier(curve, challengeService);

        var signature = signer.CreateSignature(privateKey, publicKey, digest);

        var result = verifier.Verify(signature, publicKey, digest);

        Assert.True(result);
    }

    [Fact]
    public void Verify_With_Modified_Message_Should_Return_False()
    {
        var curve = new P256CurveContext();
        var randomSource = new SystemRandomSource();
        var nonceGenerator = new SecureNonceGenerator(curve, randomSource);
        var hashService = new Sha256HashService();
        var hashToScalar = new HashToScalarService(curve, hashService);
        var challengeService = new ChallengeService(hashToScalar);
        var messageDigestService = new MessageDigestService();

        var privateKey = curve.ReduceScalar(new byte[]
        {
            0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28,
            0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F, 0x30,
            0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38,
            0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F, 0x40
        });

        var publicKey = new PublicKeyValue(curve.MultiplyBasePoint(privateKey));
        var validDigest = messageDigestService.DigestUtf8("message-1");
        var invalidDigest = messageDigestService.DigestUtf8("message-2");

        var signer = new IndividualSchnorrSigner(curve, nonceGenerator, challengeService);
        var verifier = new IndividualSchnorrVerifier(curve, challengeService);

        var signature = signer.CreateSignature(privateKey, publicKey, validDigest);

        var result = verifier.Verify(signature, publicKey, invalidDigest);

        Assert.False(result);
    }
}