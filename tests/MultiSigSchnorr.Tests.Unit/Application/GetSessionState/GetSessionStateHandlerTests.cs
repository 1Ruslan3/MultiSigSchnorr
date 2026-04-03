using MultiSigSchnorr.Application.UseCases.GetSessionState;
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
using MultiSigSchnorr.Protocol.Models;
using MultiSigSchnorr.Protocol.Sessions;
using Xunit;

namespace MultiSigSchnorr.Tests.Unit.Application.GetSessionState;

public sealed class GetSessionStateHandlerTests
{
    [Fact]
    public void Handle_Should_Return_Initial_Commitment_State()
    {
        var context = BuildContext(phase: TestPhase.CommitmentsOnly);
        var handler = new GetSessionStateHandler();

        var request = new GetSessionStateRequest
        {
            SessionId = context.Session.SessionId
        };

        var result = handler.Handle(request, context.Session);

        Assert.Equal(context.Session.SessionId, result.SessionId);
        Assert.Equal(SessionStatus.NonceRevealCollection, result.SessionStatus);
        Assert.True(result.AllCommitmentsPublished);
        Assert.False(result.AllNoncesRevealed);
        Assert.False(result.AllPartialSignaturesSubmitted);
        Assert.NotNull(result.Participants);
        Assert.Equal(3, result.Participants.Count);
        Assert.All(result.Participants, x => Assert.True(x.HasCommitment));
        Assert.All(result.Participants, x => Assert.False(x.HasReveal));
    }

    [Fact]
    public void Handle_Should_Return_Final_State_After_Protocol_Completion()
    {
        var context = BuildContext(phase: TestPhase.Completed);
        var handler = new GetSessionStateHandler();

        var request = new GetSessionStateRequest
        {
            SessionId = context.Session.SessionId
        };

        var result = handler.Handle(request, context.Session);

        Assert.Equal(SessionStatus.Completed, result.SessionStatus);
        Assert.True(result.AllCommitmentsPublished);
        Assert.True(result.AllNoncesRevealed);
        Assert.True(result.AllPartialSignaturesSubmitted);
        Assert.NotNull(result.AggregateNoncePointHex);
        Assert.NotNull(result.ChallengeHex);
        Assert.NotNull(result.AggregateSignatureNoncePointHex);
        Assert.NotNull(result.AggregateSignatureScalarHex);
        Assert.All(result.Participants, x => Assert.True(x.HasPartialSignature));
    }

    [Fact]
    public void Handle_Should_Throw_When_Request_SessionId_Does_Not_Match()
    {
        var context = BuildContext(phase: TestPhase.CommitmentsOnly);
        var handler = new GetSessionStateHandler();

        var request = new GetSessionStateRequest
        {
            SessionId = Guid.NewGuid()
        };

        Assert.Throws<InvalidOperationException>(() =>
            handler.Handle(request, context.Session));
    }

    private static TestContext BuildContext(TestPhase phase)
    {
        var protocolService = CreateProtocolService(out var publicKeyGenerationService, out var digestService, out var curve);

        var p1Id = Guid.NewGuid();
        var p2Id = Guid.NewGuid();
        var p3Id = Guid.NewGuid();

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

        var privateKeys = new Dictionary<Guid, MultiSigSchnorr.Domain.ValueObjects.ScalarValue>
        {
            [p1Id] = k1,
            [p2Id] = k2,
            [p3Id] = k3
        };

        var p1 = new Participant(p1Id, "Participant-1", publicKeyGenerationService.DerivePublicKey(k1), ParticipantStatus.Active, DateTime.UtcNow);
        var p2 = new Participant(p2Id, "Participant-2", publicKeyGenerationService.DerivePublicKey(k2), ParticipantStatus.Active, DateTime.UtcNow);
        var p3 = new Participant(p3Id, "Participant-3", publicKeyGenerationService.DerivePublicKey(k3), ParticipantStatus.Active, DateTime.UtcNow);

        var epoch = new Epoch(Guid.NewGuid(), 1, DateTime.UtcNow);
        epoch.Activate(DateTime.UtcNow);

        var members = new List<EpochMember>
        {
            new(Guid.NewGuid(), epoch.Id, p1.Id, DateTime.UtcNow),
            new(Guid.NewGuid(), epoch.Id, p2.Id, DateTime.UtcNow),
            new(Guid.NewGuid(), epoch.Id, p3.Id, DateTime.UtcNow)
        };

        var digest = digestService.DigestUtf8("get-session-state");

        var session = protocolService.CreateSession(
            epoch,
            new[] { p1, p2, p3 },
            members,
            privateKeys,
            digest,
            DateTime.UtcNow);

        var ids = new[] { p1Id, p2Id, p3Id };

        foreach (var id in ids)
            protocolService.PublishCommitment(session, id, DateTime.UtcNow);

        if (phase >= TestPhase.Reveals)
        {
            foreach (var id in ids)
                protocolService.RevealNonce(session, id, DateTime.UtcNow);
        }

        if (phase >= TestPhase.Completed)
        {
            foreach (var id in ids)
                protocolService.SubmitPartialSignature(session, id, DateTime.UtcNow);
        }

        return new TestContext(session);
    }

    private static NPartyCommitmentProtocolService CreateProtocolService(
        out PublicKeyGenerationService publicKeyGenerationService,
        out MessageDigestService digestService,
        out P256CurveContext curve)
    {
        curve = new P256CurveContext();
        var randomSource = new SystemRandomSource();
        var nonceGenerator = new SecureNonceGenerator(curve, randomSource);

        var sha256 = new Sha256HashService();
        var hashToScalar = new HashToScalarService(curve, sha256);
        var challengeService = new ChallengeService(hashToScalar);
        var commitmentService = new CommitmentService(sha256);
        var partialSignatureService = new PartialSignatureService(curve);
        var aggregateVerifier = new AggregateSignatureVerifier(curve, challengeService);
        publicKeyGenerationService = new PublicKeyGenerationService(curve);
        var aggregateKeyService = new AggregateKeyService(curve, hashToScalar);
        var epochGuard = new EpochParticipationGuard();
        digestService = new MessageDigestService();

        return new NPartyCommitmentProtocolService(
            publicKeyGenerationService,
            aggregateKeyService,
            nonceGenerator,
            commitmentService,
            challengeService,
            partialSignatureService,
            aggregateVerifier,
            curve,
            epochGuard);
    }

    private sealed class TestContext
    {
        public NPartyProtocolSession Session { get; }

        public TestContext(NPartyProtocolSession session)
        {
            Session = session;
        }
    }

    private enum TestPhase
    {
        CommitmentsOnly = 0,
        Reveals = 1,
        Completed = 2
    }
}