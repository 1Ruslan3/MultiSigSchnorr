using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Crypto.Curves;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Api.Development;

public sealed class DevelopmentDataSeeder
{
    private readonly IEpochRepository _epochRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IEpochMemberRepository _epochMemberRepository;
    private readonly IPrivateKeyMaterialRepository _privateKeyMaterialRepository;
    private readonly PublicKeyGenerationService _publicKeyGenerationService;

    public DevelopmentSeedSnapshot? Snapshot { get; private set; }

    public DevelopmentDataSeeder(
        IEpochRepository epochRepository,
        IParticipantRepository participantRepository,
        IEpochMemberRepository epochMemberRepository,
        IPrivateKeyMaterialRepository privateKeyMaterialRepository,
        PublicKeyGenerationService publicKeyGenerationService)
    {
        _epochRepository = epochRepository ?? throw new ArgumentNullException(nameof(epochRepository));
        _participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
        _epochMemberRepository = epochMemberRepository ?? throw new ArgumentNullException(nameof(epochMemberRepository));
        _privateKeyMaterialRepository = privateKeyMaterialRepository ?? throw new ArgumentNullException(nameof(privateKeyMaterialRepository));
        _publicKeyGenerationService = publicKeyGenerationService ?? throw new ArgumentNullException(nameof(publicKeyGenerationService));
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (Snapshot is not null)
            return;

        var epochId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var participant1Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
        var participant2Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2");
        var participant3Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3");

        var existingEpoch = await _epochRepository.GetByIdAsync(epochId, cancellationToken);
        if (existingEpoch is not null)
        {
            Snapshot = new DevelopmentSeedSnapshot
            {
                EpochId = epochId,
                EpochNumber = existingEpoch.Number,
                Participant1Id = participant1Id,
                Participant2Id = participant2Id,
                Participant3Id = participant3Id
            };

            return;
        }

        var privateKey1 = ScalarValue.FromHex("101112131415161718191A1B1C1D1E1F202122232425262728292A2B2C2D2E2F");
        var privateKey2 = ScalarValue.FromHex("3132333435363738393A3B3C3D3E3F404142434445464748494A4B4C4D4E4F50");
        var privateKey3 = ScalarValue.FromHex("5152535455565758595A5B5C5D5E5F606162636465666768696A6B6C6D6E6F70");

        var participant1 = new Participant(
            participant1Id,
            "Participant-1",
            _publicKeyGenerationService.DerivePublicKey(privateKey1),
            ParticipantStatus.Active,
            DateTime.UtcNow);

        var participant2 = new Participant(
            participant2Id,
            "Participant-2",
            _publicKeyGenerationService.DerivePublicKey(privateKey2),
            ParticipantStatus.Active,
            DateTime.UtcNow);

        var participant3 = new Participant(
            participant3Id,
            "Participant-3",
            _publicKeyGenerationService.DerivePublicKey(privateKey3),
            ParticipantStatus.Active,
            DateTime.UtcNow);

        var epoch = new Epoch(
            epochId,
            1,
            DateTime.UtcNow);

        epoch.Activate(DateTime.UtcNow);

        await _participantRepository.AddAsync(participant1, cancellationToken);
        await _participantRepository.AddAsync(participant2, cancellationToken);
        await _participantRepository.AddAsync(participant3, cancellationToken);

        await _privateKeyMaterialRepository.SetAsync(participant1.Id, privateKey1, cancellationToken);
        await _privateKeyMaterialRepository.SetAsync(participant2.Id, privateKey2, cancellationToken);
        await _privateKeyMaterialRepository.SetAsync(participant3.Id, privateKey3, cancellationToken);

        await _epochRepository.AddAsync(epoch, cancellationToken);

        await _epochMemberRepository.AddAsync(
            new EpochMember(Guid.NewGuid(), epoch.Id, participant1.Id, DateTime.UtcNow),
            cancellationToken);

        await _epochMemberRepository.AddAsync(
            new EpochMember(Guid.NewGuid(), epoch.Id, participant2.Id, DateTime.UtcNow),
            cancellationToken);

        await _epochMemberRepository.AddAsync(
            new EpochMember(Guid.NewGuid(), epoch.Id, participant3.Id, DateTime.UtcNow),
            cancellationToken);

        Snapshot = new DevelopmentSeedSnapshot
        {
            EpochId = epoch.Id,
            EpochNumber = epoch.Number,
            Participant1Id = participant1.Id,
            Participant2Id = participant2.Id,
            Participant3Id = participant3.Id
        };
    }
}