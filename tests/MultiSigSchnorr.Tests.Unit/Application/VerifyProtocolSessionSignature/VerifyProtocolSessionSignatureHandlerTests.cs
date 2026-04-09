using MultiSigSchnorr.Application.UseCases.VerifyProtocolSessionSignature;
using MultiSigSchnorr.Crypto.Aggregation;
using MultiSigSchnorr.Crypto.Commitments;
using MultiSigSchnorr.Crypto.Curves;
using MultiSigSchnorr.Crypto.Hashing;
using MultiSigSchnorr.Crypto.Nonces;
using MultiSigSchnorr.Crypto.Schnorr;
using MultiSigSchnorr.Crypto.Security;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Infrastructure.Repositories;
using MultiSigSchnorr.Protocol.Epochs;
using MultiSigSchnorr.Protocol.Sessions;
using Xunit;

namespace MultiSigSchnorr.Tests.Unit.Application.VerifyProtocolSessionSignature;

public sealed class VerifyProtocolSessionSignatureHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Return_Valid_For_Completed_Session()
    {
        var context = await BuildContextAsync(true);

        var handler = new VerifyProtocolSessionSignatureHandler(
            context.ProtocolSessionRepository,
            context.AggregateSignatureVerifier);

        var result = await handler.HandleAsync(
            new VerifyProtocolSessionSignatureRequest
            {
                SessionId = context.Session.SessionId
            });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Invalid_When_AggregateSignature_Is_Missing()
    {
        var context = await BuildContextAsync(false);

        var handler = new VerifyProtocolSessionSignatureHandler(
            context.ProtocolSessionRepository,
            context.AggregateSignatureVerifier);

        var result = await handler.HandleAsync(
            new VerifyProtocolSessionSignatureRequest
            {
                SessionId = context.Session.SessionId
            });

        Assert.False(result.IsValid);
        Assert.Equal("Aggregate signature is not available yet.", result.Message);
    }

    private static async Task<TestContext> BuildContextAsync(bool completeSession)
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
        var epochGuard = new EpochParticipationGuard();

        var protocolService = new NPartyCommitmentProtocolService(
            publicKeyGenerationService,
            aggregateKeyService,
            nonceGenerator,
            commitmentService,
            challengeService,
            partialSignatureService,
            aggregateSignatureVerifier,
            curve,
            epochGuard);

        var protocolSessionRepository = new InMemoryProtocolSessionRepository();

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

        var digest = new MessageDigestService().DigestUtf8("verify-protocol-session");

        var session = protocolService.CreateSession(
            epoch,
            new[] { p1, p2, p3 },
            members,
            privateKeys,
            digest,
            DateTime.UtcNow);

        foreach (var id in new[] { p1Id, p2Id, p3Id })
            protocolService.PublishCommitment(session, id, DateTime.UtcNow);

        foreach (var id in new[] { p1Id, p2Id, p3Id })
            protocolService.RevealNonce(session, id, DateTime.UtcNow);

        if (completeSession)
        {
            foreach (var id in new[] { p1Id, p2Id, p3Id })
                protocolService.SubmitPartialSignature(session, id, DateTime.UtcNow);
        }

        await protocolSessionRepository.AddAsync(session);

        return new TestContext(protocolSessionRepository, aggregateSignatureVerifier, session);
    }

    private sealed class TestContext
    {
        public InMemoryProtocolSessionRepository ProtocolSessionRepository { get; }
        public AggregateSignatureVerifier AggregateSignatureVerifier { get; }
        public MultiSigSchnorr.Protocol.Models.NPartyProtocolSession Session { get; }

        public TestContext(
            InMemoryProtocolSessionRepository protocolSessionRepository,
            AggregateSignatureVerifier aggregateSignatureVerifier,
            MultiSigSchnorr.Protocol.Models.NPartyProtocolSession session)
        {
            ProtocolSessionRepository = protocolSessionRepository;
            AggregateSignatureVerifier = aggregateSignatureVerifier;
            Session = session;
        }
    }
}