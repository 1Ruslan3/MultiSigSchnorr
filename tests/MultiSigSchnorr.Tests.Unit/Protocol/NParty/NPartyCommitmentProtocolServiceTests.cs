using MultiSigSchnorr.Crypto.Aggregation;
using MultiSigSchnorr.Crypto.Commitments;
using MultiSigSchnorr.Crypto.Curves;
using MultiSigSchnorr.Crypto.Hashing;
using MultiSigSchnorr.Crypto.Nonces;
using MultiSigSchnorr.Crypto.Schnorr;
using MultiSigSchnorr.Crypto.Security;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Protocol.Epochs;
using MultiSigSchnorr.Protocol.Sessions;
using Xunit;

namespace MultiSigSchnorr.Tests.Unit.Protocol.NParty;

public sealed class NPartyCommitmentProtocolServiceTests
{
    [Fact]
    public void Full_NParty_Protocol_Should_Complete_For_Three_Participants()
    {
        var curve = new P256CurveContext();
        var randomSource = new SystemRandomSource();
        var nonceGenerator = new SecureNonceGenerator(curve, randomSource);

        var sha256 = new Sha256HashService();
        var hashToScalar = new HashToScalarService(curve, sha256);
        var challengeService = new ChallengeService(hashToScalar);
        var commitmentService = new CommitmentService(sha256);
        var partialSignatureService = new PartialSignatureService(curve);
        var aggregateVerifier = new AggregateSignatureVerifier(curve, challengeService);
        var publicKeyGenerationService = new PublicKeyGenerationService(curve);
        var aggregateKeyService = new AggregateKeyService(curve, hashToScalar);
        var digestService = new MessageDigestService();
        var epochGuard = new EpochParticipationGuard();

        var protocolService = new NPartyCommitmentProtocolService(
            publicKeyGenerationService,
            aggregateKeyService,
            nonceGenerator,
            commitmentService,
            challengeService,
            partialSignatureService,
            aggregateVerifier,
            curve,
            epochGuard);

        var privateKeys = new Dictionary<Guid, MultiSigSchnorr.Domain.ValueObjects.ScalarValue>();

        var k1 = curve.ReduceScalar(new byte[]
        {
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
            0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
            0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
            0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F
        });

        var k2 = curve.ReduceScalar(new byte[]
        {
            0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38,
            0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F, 0x40,
            0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
            0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F, 0x50
        });

        var k3 = curve.ReduceScalar(new byte[]
        {
            0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58,
            0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F, 0x60,
            0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68,
            0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F, 0x70
        });

        var p1Id = Guid.NewGuid();
        var p2Id = Guid.NewGuid();
        var p3Id = Guid.NewGuid();

        privateKeys[p1Id] = k1;
        privateKeys[p2Id] = k2;
        privateKeys[p3Id] = k3;

        var p1 = new Participant(
            p1Id,
            "Participant-1",
            publicKeyGenerationService.DerivePublicKey(k1),
            ParticipantStatus.Active,
            DateTime.UtcNow);

        var p2 = new Participant(
            p2Id,
            "Participant-2",
            publicKeyGenerationService.DerivePublicKey(k2),
            ParticipantStatus.Active,
            DateTime.UtcNow);

        var p3 = new Participant(
            p3Id,
            "Participant-3",
            publicKeyGenerationService.DerivePublicKey(k3),
            ParticipantStatus.Active,
            DateTime.UtcNow);

        var epoch = new Epoch(Guid.NewGuid(), 1, DateTime.UtcNow);
        epoch.Activate(DateTime.UtcNow);

        var members = new List<EpochMember>
        {
            new(Guid.NewGuid(), epoch.Id, p1.Id, DateTime.UtcNow),
            new(Guid.NewGuid(), epoch.Id, p2.Id, DateTime.UtcNow),
            new(Guid.NewGuid(), epoch.Id, p3.Id, DateTime.UtcNow)
        };

        var digest = digestService.DigestUtf8("n-party-protocol");

        var session = protocolService.CreateSession(
            epoch,
            new[] { p1, p2, p3 },
            members,
            privateKeys,
            digest,
            DateTime.UtcNow);

        foreach (var participantId in session.Participants.Keys)
            protocolService.PublishCommitment(session, participantId, DateTime.UtcNow);

        foreach (var participantId in session.Participants.Keys)
            protocolService.RevealNonce(session, participantId, DateTime.UtcNow);

        foreach (var participantId in session.Participants.Keys)
            protocolService.SubmitPartialSignature(session, participantId, DateTime.UtcNow);

        Assert.NotNull(session.AggregateSignature);
        Assert.Equal(SessionStatus.Completed, session.SigningSession.Status);
    }

    [Fact]
    public void CreateSession_With_Revoked_Participant_Should_Throw()
    {
        var curve = new P256CurveContext();
        var randomSource = new SystemRandomSource();
        var nonceGenerator = new SecureNonceGenerator(curve, randomSource);

        var sha256 = new Sha256HashService();
        var hashToScalar = new HashToScalarService(curve, sha256);
        var challengeService = new ChallengeService(hashToScalar);
        var commitmentService = new CommitmentService(sha256);
        var partialSignatureService = new PartialSignatureService(curve);
        var aggregateVerifier = new AggregateSignatureVerifier(curve, challengeService);
        var publicKeyGenerationService = new PublicKeyGenerationService(curve);
        var aggregateKeyService = new AggregateKeyService(curve, hashToScalar);
        var digestService = new MessageDigestService();
        var epochGuard = new EpochParticipationGuard();

        var protocolService = new NPartyCommitmentProtocolService(
            publicKeyGenerationService,
            aggregateKeyService,
            nonceGenerator,
            commitmentService,
            challengeService,
            partialSignatureService,
            aggregateVerifier,
            curve,
            epochGuard);

        var k1 = curve.ReduceScalar(new byte[]
        {
            0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78,
            0x79, 0x7A, 0x7B, 0x7C, 0x7D, 0x7E, 0x7F, 0x80,
            0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88,
            0x89, 0x8A, 0x8B, 0x8C, 0x8D, 0x8E, 0x8F, 0x90
        });

        var k2 = curve.ReduceScalar(new byte[]
        {
            0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98,
            0x99, 0x9A, 0x9B, 0x9C, 0x9D, 0x9E, 0x9F, 0xA0,
            0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8,
            0xA9, 0xAA, 0xAB, 0xAC, 0xAD, 0xAE, 0xAF, 0xB0
        });

        var p1Id = Guid.NewGuid();
        var p2Id = Guid.NewGuid();

        var privateKeys = new Dictionary<Guid, MultiSigSchnorr.Domain.ValueObjects.ScalarValue>
        {
            [p1Id] = k1,
            [p2Id] = k2
        };

        var activeParticipant = new Participant(
            p1Id,
            "Active-Participant",
            publicKeyGenerationService.DerivePublicKey(k1),
            ParticipantStatus.Active,
            DateTime.UtcNow);

        var revokedParticipant = new Participant(
            p2Id,
            "Revoked-Participant",
            publicKeyGenerationService.DerivePublicKey(k2),
            ParticipantStatus.Revoked,
            DateTime.UtcNow);

        var epoch = new Epoch(Guid.NewGuid(), 2, DateTime.UtcNow);
        epoch.Activate(DateTime.UtcNow);

        var members = new List<EpochMember>
        {
            new(Guid.NewGuid(), epoch.Id, activeParticipant.Id, DateTime.UtcNow),
            new(Guid.NewGuid(), epoch.Id, revokedParticipant.Id, DateTime.UtcNow)
        };

        var digest = digestService.DigestUtf8("revoked-participant-test");

        Assert.Throws<InvalidOperationException>(() =>
            protocolService.CreateSession(
                epoch,
                new[] { activeParticipant, revokedParticipant },
                members,
                privateKeys,
                digest,
                DateTime.UtcNow));
    }
}