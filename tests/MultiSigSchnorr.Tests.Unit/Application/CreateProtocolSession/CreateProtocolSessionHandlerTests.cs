using MultiSigSchnorr.Application.Audit;
using MultiSigSchnorr.Application.UseCases.CreateProtocolSession;
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

namespace MultiSigSchnorr.Tests.Unit.Application.CreateProtocolSession;

public sealed class CreateProtocolSessionHandlerTests
{
    [Fact]
    public async Task HandleAsync_Should_Create_And_Save_ProtocolSession()
    {
        var context = await BuildContextAsync();
        var handler = CreateHandler(context);

        var request = new CreateProtocolSessionRequest
        {
            EpochId = context.Epoch.Id,
            ParticipantIds = new[]
            {
                context.Participant1.Id,
                context.Participant2.Id,
                context.Participant3.Id
            },
            Message = "create-protocol-session"
        };

        var session = await handler.HandleAsync(request, DateTime.UtcNow);
        var loaded = await context.ProtocolSessionRepository.GetByIdAsync(session.SessionId);

        Assert.NotNull(loaded);
        Assert.Equal(context.Epoch.Id, session.Epoch.Id);
        Assert.Equal(3, session.Participants.Count);
        Assert.Equal(SessionStatus.Created, session.SigningSession.Status);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_When_PrivateKey_Material_Is_Missing()
    {
        var context = await BuildContextAsync(includeThirdPrivateKey: false);
        var handler = CreateHandler(context);

        var request = new CreateProtocolSessionRequest
        {
            EpochId = context.Epoch.Id,
            ParticipantIds = new[]
            {
                context.Participant1.Id,
                context.Participant2.Id,
                context.Participant3.Id
            },
            Message = "missing-private-key"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(request, DateTime.UtcNow));
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_When_Epoch_Is_Not_Found()
    {
        var context = await BuildContextAsync();
        var handler = CreateHandler(context);

        var request = new CreateProtocolSessionRequest
        {
            EpochId = Guid.NewGuid(),
            ParticipantIds = new[]
            {
                context.Participant1.Id,
                context.Participant2.Id
            },
            Message = "unknown-epoch"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(request, DateTime.UtcNow));
    }

    private static CreateProtocolSessionHandler CreateHandler(TestContext context)
    {
        var auditLogService = new AuditLogService(
            new InMemoryAuditLogRepository());

        return new CreateProtocolSessionHandler(
            context.EpochRepository,
            context.ParticipantRepository,
            context.EpochMemberRepository,
            context.PrivateKeyMaterialRepository,
            context.ProtocolSessionRepository,
            context.ProtocolService,
            context.MessageDigestService,
            auditLogService);
    }

    private static async Task<TestContext> BuildContextAsync(
        bool includeThirdPrivateKey = true)
    {
        var epochRepository = new InMemoryEpochRepository();
        var participantRepository = new InMemoryParticipantRepository();
        var epochMemberRepository = new InMemoryEpochMemberRepository();
        var privateKeyMaterialRepository = new InMemoryPrivateKeyMaterialRepository();
        var protocolSessionRepository = new InMemoryProtocolSessionRepository();

        var protocolService = CreateProtocolService(
            out var publicKeyGenerationService,
            out var messageDigestService,
            out var curve);

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

        var participant1 = new Participant(
            Guid.NewGuid(),
            "Participant-1",
            publicKeyGenerationService.DerivePublicKey(k1),
            ParticipantStatus.Active,
            DateTime.UtcNow);

        var participant2 = new Participant(
            Guid.NewGuid(),
            "Participant-2",
            publicKeyGenerationService.DerivePublicKey(k2),
            ParticipantStatus.Active,
            DateTime.UtcNow);

        var participant3 = new Participant(
            Guid.NewGuid(),
            "Participant-3",
            publicKeyGenerationService.DerivePublicKey(k3),
            ParticipantStatus.Active,
            DateTime.UtcNow);

        await participantRepository.AddAsync(participant1);
        await participantRepository.AddAsync(participant2);
        await participantRepository.AddAsync(participant3);

        await privateKeyMaterialRepository.SetAsync(participant1.Id, k1);
        await privateKeyMaterialRepository.SetAsync(participant2.Id, k2);

        if (includeThirdPrivateKey)
            await privateKeyMaterialRepository.SetAsync(participant3.Id, k3);

        var epoch = new Epoch(Guid.NewGuid(), 1, DateTime.UtcNow);
        epoch.Activate(DateTime.UtcNow);

        await epochRepository.AddAsync(epoch);

        await epochMemberRepository.AddAsync(
            new EpochMember(
                Guid.NewGuid(),
                epoch.Id,
                participant1.Id,
                DateTime.UtcNow));

        await epochMemberRepository.AddAsync(
            new EpochMember(
                Guid.NewGuid(),
                epoch.Id,
                participant2.Id,
                DateTime.UtcNow));

        await epochMemberRepository.AddAsync(
            new EpochMember(
                Guid.NewGuid(),
                epoch.Id,
                participant3.Id,
                DateTime.UtcNow));

        return new TestContext(
            epochRepository,
            participantRepository,
            epochMemberRepository,
            privateKeyMaterialRepository,
            protocolSessionRepository,
            protocolService,
            messageDigestService,
            epoch,
            participant1,
            participant2,
            participant3);
    }

    private static NPartyCommitmentProtocolService CreateProtocolService(
        out PublicKeyGenerationService publicKeyGenerationService,
        out MessageDigestService messageDigestService,
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

        messageDigestService = new MessageDigestService();

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
        public InMemoryEpochRepository EpochRepository { get; }
        public InMemoryParticipantRepository ParticipantRepository { get; }
        public InMemoryEpochMemberRepository EpochMemberRepository { get; }
        public InMemoryPrivateKeyMaterialRepository PrivateKeyMaterialRepository { get; }
        public InMemoryProtocolSessionRepository ProtocolSessionRepository { get; }
        public NPartyCommitmentProtocolService ProtocolService { get; }
        public MessageDigestService MessageDigestService { get; }

        public Epoch Epoch { get; }
        public Participant Participant1 { get; }
        public Participant Participant2 { get; }
        public Participant Participant3 { get; }

        public TestContext(
            InMemoryEpochRepository epochRepository,
            InMemoryParticipantRepository participantRepository,
            InMemoryEpochMemberRepository epochMemberRepository,
            InMemoryPrivateKeyMaterialRepository privateKeyMaterialRepository,
            InMemoryProtocolSessionRepository protocolSessionRepository,
            NPartyCommitmentProtocolService protocolService,
            MessageDigestService messageDigestService,
            Epoch epoch,
            Participant participant1,
            Participant participant2,
            Participant participant3)
        {
            EpochRepository = epochRepository;
            ParticipantRepository = participantRepository;
            EpochMemberRepository = epochMemberRepository;
            PrivateKeyMaterialRepository = privateKeyMaterialRepository;
            ProtocolSessionRepository = protocolSessionRepository;
            ProtocolService = protocolService;
            MessageDigestService = messageDigestService;
            Epoch = epoch;
            Participant1 = participant1;
            Participant2 = participant2;
            Participant3 = participant3;
        }
    }
}