using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Order;
using MultiSigSchnorr.Crypto.Aggregation;
using MultiSigSchnorr.Crypto.Commitments;
using MultiSigSchnorr.Crypto.Curves;
using MultiSigSchnorr.Crypto.Hashing;
using MultiSigSchnorr.Crypto.Nonces;
using MultiSigSchnorr.Crypto.Schnorr;
using MultiSigSchnorr.Crypto.Security;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Domain.ValueObjects;
using MultiSigSchnorr.Protocol.Epochs;
using MultiSigSchnorr.Protocol.Sessions;

namespace MultiSigSchnorr.Benchmarks;

[MemoryDiagnoser]
[RankColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 10)]
public class SignatureProtectionBenchmarks
{
    private P256CurveContext _curve = default!;
    private PublicKeyGenerationService _publicKeyGenerationService = default!;
    private PartialSignatureService _partialSignatureService = default!;
    private NPartyCommitmentProtocolService _protocolService = default!;
    private MessageDigestService _messageDigestService = default!;

    private ScalarValue _k1 = default!;
    private ScalarValue _k2 = default!;
    private ScalarValue _k3 = default!;

    private ScalarValue _nonce = default!;
    private ScalarValue _challenge = default!;
    private ScalarValue _aggregationCoefficient = default!;

    [GlobalSetup]
    public void Setup()
    {
        _curve = new P256CurveContext();

        var randomSource = new SystemRandomSource();
        var nonceGenerator = new SecureNonceGenerator(_curve, randomSource);

        var sha256 = new Sha256HashService();
        var hashToScalar = new HashToScalarService(_curve, sha256);
        var challengeService = new ChallengeService(hashToScalar);
        var commitmentService = new CommitmentService(sha256);
        var aggregateVerifier = new AggregateSignatureVerifier(_curve, challengeService);
        var aggregateKeyService = new AggregateKeyService(_curve, hashToScalar);
        var epochGuard = new EpochParticipationGuard();

        _publicKeyGenerationService = new PublicKeyGenerationService(_curve);
        _partialSignatureService = new PartialSignatureService(_curve);
        _messageDigestService = new MessageDigestService();

        _protocolService = new NPartyCommitmentProtocolService(
            _publicKeyGenerationService,
            aggregateKeyService,
            nonceGenerator,
            commitmentService,
            challengeService,
            _partialSignatureService,
            aggregateVerifier,
            _curve,
            epochGuard);

        _k1 = _curve.ReduceScalar(new byte[]
        {
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
            0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
            0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
            0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F
        });

        _k2 = _curve.ReduceScalar(new byte[]
        {
            0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38,
            0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F, 0x40,
            0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
            0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E, 0x4F, 0x50
        });

        _k3 = _curve.ReduceScalar(new byte[]
        {
            0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58,
            0x59, 0x5A, 0x5B, 0x5C, 0x5D, 0x5E, 0x5F, 0x60,
            0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68,
            0x69, 0x6A, 0x6B, 0x6C, 0x6D, 0x6E, 0x6F, 0x70
        });

        _nonce = ScalarValue.FromHex(
            "7172737475767778797A7B7C7D7E7F808182838485868788898A8B8C8D8E8F90");

        _challenge = ScalarValue.FromHex(
            "0BC578F7D4673385412294724AF7ED73D57F200EC57E43B44700AF16C411590A");

        _aggregationCoefficient = ScalarValue.FromHex(
            "DA072C9A813DD48348D0AC9BF8D76FA773DC5868C3962B631EFA19E010D839AE");
    }

    [Benchmark]
    public string PublicKey_Baseline()
    {
        return _publicKeyGenerationService
            .DerivePublicKey(_k1, SignatureProtectionMode.Baseline)
            .ToHex();
    }

    [Benchmark]
    public string PublicKey_Randomized()
    {
        return _publicKeyGenerationService
            .DerivePublicKey(_k1, SignatureProtectionMode.RandomizedScalarProcessing)
            .ToHex();
    }

    [Benchmark]
    public string PartialSignature_Baseline()
    {
        return _partialSignatureService
            .CreatePartialSignature(
                _nonce,
                _k1,
                _challenge,
                _aggregationCoefficient,
                SignatureProtectionMode.Baseline)
            .ToHex();
    }

    [Benchmark]
    public string PartialSignature_Randomized()
    {
        return _partialSignatureService
            .CreatePartialSignature(
                _nonce,
                _k1,
                _challenge,
                _aggregationCoefficient,
                SignatureProtectionMode.RandomizedScalarProcessing)
            .ToHex();
    }

    [Benchmark]
    public string FullProtocol_Baseline()
    {
        return RunFullProtocol(SignatureProtectionMode.Baseline);
    }

    [Benchmark]
    public string FullProtocol_Randomized()
    {
        return RunFullProtocol(SignatureProtectionMode.RandomizedScalarProcessing);
    }

    private string RunFullProtocol(SignatureProtectionMode mode)
    {
        var p1Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
        var p2Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2");
        var p3Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3");

        var p1 = new Participant(
            p1Id,
            "Participant-1",
            _publicKeyGenerationService.DerivePublicKey(_k1, SignatureProtectionMode.Baseline),
            ParticipantStatus.Active,
            DateTime.UtcNow);

        var p2 = new Participant(
            p2Id,
            "Participant-2",
            _publicKeyGenerationService.DerivePublicKey(_k2, SignatureProtectionMode.Baseline),
            ParticipantStatus.Active,
            DateTime.UtcNow);

        var p3 = new Participant(
            p3Id,
            "Participant-3",
            _publicKeyGenerationService.DerivePublicKey(_k3, SignatureProtectionMode.Baseline),
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

        var privateKeys = new Dictionary<Guid, ScalarValue>
        {
            [p1.Id] = _k1,
            [p2.Id] = _k2,
            [p3.Id] = _k3
        };

        var digest = _messageDigestService.DigestUtf8($"benchmark-{mode}");

        var session = _protocolService.CreateSession(
            epoch,
            new[] { p1, p2, p3 },
            members,
            privateKeys,
            digest,
            DateTime.UtcNow,
            mode);

        foreach (var participantId in new[] { p1.Id, p2.Id, p3.Id })
            _protocolService.PublishCommitment(session, participantId, DateTime.UtcNow);

        foreach (var participantId in new[] { p1.Id, p2.Id, p3.Id })
            _protocolService.RevealNonce(session, participantId, DateTime.UtcNow);

        foreach (var participantId in new[] { p1.Id, p2.Id, p3.Id })
            _protocolService.SubmitPartialSignature(session, participantId, DateTime.UtcNow);

        return session.AggregateSignature?.SignatureScalar.ToHex() ?? string.Empty;
    }
}