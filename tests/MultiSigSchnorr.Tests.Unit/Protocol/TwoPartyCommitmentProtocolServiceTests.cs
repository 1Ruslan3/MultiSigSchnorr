using MultiSigSchnorr.Crypto.Aggregation;
using MultiSigSchnorr.Crypto.Commitments;
using MultiSigSchnorr.Crypto.Curves;
using MultiSigSchnorr.Crypto.Hashing;
using MultiSigSchnorr.Crypto.Nonces;
using MultiSigSchnorr.Crypto.Schnorr;
using MultiSigSchnorr.Crypto.Security;
using MultiSigSchnorr.Protocol.Sessions;
using Xunit;

namespace MultiSigSchnorr.Tests.Unit.Protocol;

public sealed class TwoPartyCommitmentProtocolServiceTests
{
    [Fact]
    public void Full_TwoParty_Commitment_Reveal_Protocol_Should_Complete()
    {
        var curve = new P256CurveContext();
        var randomSource = new SystemRandomSource();
        var nonceGenerator = new SecureNonceGenerator(curve, randomSource);

        var sha256 = new Sha256HashService();
        var hashToScalar = new HashToScalarService(curve, sha256);
        var challengeService = new ChallengeService(hashToScalar);
        var commitmentService = new CommitmentService(sha256);
        var partialSignatureService = new PartialSignatureService(curve);
        var aggregateSignatureVerifier = new AggregateSignatureVerifier(curve, challengeService);
        var publicKeyGenerationService = new PublicKeyGenerationService(curve);
        var aggregateKeyService = new AggregateKeyService(curve, hashToScalar);
        var digestService = new MessageDigestService();

        var protocolService = new TwoPartyCommitmentProtocolService(
            publicKeyGenerationService,
            aggregateKeyService,
            nonceGenerator,
            commitmentService,
            challengeService,
            partialSignatureService,
            aggregateSignatureVerifier,
            curve);

        var firstPrivateKey = curve.ReduceScalar(new byte[]
        {
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
            0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
            0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
            0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F
        });

        var secondPrivateKey = curve.ReduceScalar(new byte[]
        {
            0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38,
            0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F, 0x40,
            0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
            0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F, 0x50
        });

        var digest = digestService.DigestUtf8("protocol-two-party-signature");

        var session = protocolService.CreateSession(
            Guid.NewGuid(),
            firstPrivateKey,
            secondPrivateKey,
            digest,
            DateTime.UtcNow);

        protocolService.PublishCommitment(session, session.FirstParticipant.ParticipantId, DateTime.UtcNow);
        protocolService.PublishCommitment(session, session.SecondParticipant.ParticipantId, DateTime.UtcNow);

        protocolService.RevealNonce(session, session.FirstParticipant.ParticipantId, DateTime.UtcNow);
        protocolService.RevealNonce(session, session.SecondParticipant.ParticipantId, DateTime.UtcNow);

        protocolService.SubmitPartialSignature(session, session.FirstParticipant.ParticipantId, DateTime.UtcNow);
        protocolService.SubmitPartialSignature(session, session.SecondParticipant.ParticipantId, DateTime.UtcNow);

        Assert.NotNull(session.AggregateSignature);
        Assert.Equal(Domain.Enums.SessionStatus.Completed, session.SigningSession.Status);
    }

    [Fact]
    public void Reveal_Before_All_Commitments_Should_Throw()
    {
        var curve = new P256CurveContext();
        var randomSource = new SystemRandomSource();
        var nonceGenerator = new SecureNonceGenerator(curve, randomSource);

        var sha256 = new Sha256HashService();
        var hashToScalar = new HashToScalarService(curve, sha256);
        var challengeService = new ChallengeService(hashToScalar);
        var commitmentService = new CommitmentService(sha256);
        var partialSignatureService = new PartialSignatureService(curve);
        var aggregateSignatureVerifier = new AggregateSignatureVerifier(curve, challengeService);
        var publicKeyGenerationService = new PublicKeyGenerationService(curve);
        var aggregateKeyService = new AggregateKeyService(curve, hashToScalar);
        var digestService = new MessageDigestService();

        var protocolService = new TwoPartyCommitmentProtocolService(
            publicKeyGenerationService,
            aggregateKeyService,
            nonceGenerator,
            commitmentService,
            challengeService,
            partialSignatureService,
            aggregateSignatureVerifier,
            curve);

        var firstPrivateKey = curve.ReduceScalar(new byte[]
        {
            0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58,
            0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F, 0x60,
            0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68,
            0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F, 0x70
        });

        var secondPrivateKey = curve.ReduceScalar(new byte[]
        {
            0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78,
            0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F, 0x80,
            0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88,
            0x89, 0x8A, 0x8B, 0x8C, 0x8D, 0x8E, 0x8F, 0x90
        });

        var digest = digestService.DigestUtf8("reveal-before-commitments");

        var session = protocolService.CreateSession(
            Guid.NewGuid(),
            firstPrivateKey,
            secondPrivateKey,
            digest,
            DateTime.UtcNow);

        protocolService.PublishCommitment(session, session.FirstParticipant.ParticipantId, DateTime.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            protocolService.RevealNonce(session, session.FirstParticipant.ParticipantId, DateTime.UtcNow));
    }
}