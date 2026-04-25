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
        var participant1Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
        var participant2Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2");
        var participant3Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3");

        var privateKey1 = ScalarValue.FromHex("101112131415161718191A1B1C1D1E1F202122232425262728292A2B2C2D2E2F");
        var privateKey2 = ScalarValue.FromHex("3132333435363738393A3B3C3D3E3F404142434445464748494A4B4C4D4E4F50");
        var privateKey3 = ScalarValue.FromHex("5152535455565758595A5B5C5D5E5F606162636465666768696A6B6C6D6E6F70");

        await _privateKeyMaterialRepository.SetAsync(participant1Id, privateKey1, cancellationToken);
        await _privateKeyMaterialRepository.SetAsync(participant2Id, privateKey2, cancellationToken);
        await _privateKeyMaterialRepository.SetAsync(participant3Id, privateKey3, cancellationToken);

        var participant1 = await EnsureParticipantAsync(
            participant1Id,
            "Participant-1",
            privateKey1,
            cancellationToken);

        var participant2 = await EnsureParticipantAsync(
            participant2Id,
            "Participant-2",
            privateKey2,
            cancellationToken);

        var participant3 = await EnsureParticipantAsync(
            participant3Id,
            "Participant-3",
            privateKey3,
            cancellationToken);

        var activeEpoch = await EnsureActiveEpochAsync(cancellationToken);

        await EnsureEpochMemberAsync(activeEpoch.Id, participant1.Id, cancellationToken);
        await EnsureEpochMemberAsync(activeEpoch.Id, participant2.Id, cancellationToken);
        await EnsureEpochMemberAsync(activeEpoch.Id, participant3.Id, cancellationToken);

        Snapshot = new DevelopmentSeedSnapshot
        {
            EpochId = activeEpoch.Id,
            EpochNumber = activeEpoch.Number,
            Participant1Id = participant1.Id,
            Participant2Id = participant2.Id,
            Participant3Id = participant3.Id
        };
    }

    private async Task<Participant> EnsureParticipantAsync(
        Guid participantId,
        string displayName,
        ScalarValue privateKey,
        CancellationToken cancellationToken)
    {
        var existing = await _participantRepository.GetByIdAsync(participantId, cancellationToken);
        if (existing is not null)
            return existing;

        var participant = new Participant(
            participantId,
            displayName,
            _publicKeyGenerationService.DerivePublicKey(privateKey),
            ParticipantStatus.Active,
            DateTime.UtcNow);

        await _participantRepository.AddAsync(participant, cancellationToken);

        return participant;
    }

    private async Task<Epoch> EnsureActiveEpochAsync(CancellationToken cancellationToken)
    {
        var epochs = await _epochRepository.ListAsync(cancellationToken);

        var activeEpoch = epochs
            .Where(x => x.Status == EpochStatus.Active)
            .OrderByDescending(x => x.Number)
            .FirstOrDefault();

        if (activeEpoch is not null)
            return activeEpoch;

        var nextNumber = epochs.Count == 0
            ? 1
            : epochs.Max(x => x.Number) + 1;

        var epochId = epochs.Count == 0
            ? Guid.Parse("11111111-1111-1111-1111-111111111111")
            : Guid.NewGuid();

        var newEpoch = new Epoch(
            epochId,
            nextNumber,
            DateTime.UtcNow);

        newEpoch.Activate(DateTime.UtcNow);

        await _epochRepository.AddAsync(newEpoch, cancellationToken);

        return newEpoch;
    }

    private async Task EnsureEpochMemberAsync(
        Guid epochId,
        Guid participantId,
        CancellationToken cancellationToken)
    {
        var members = await _epochMemberRepository.GetByEpochIdAsync(epochId, cancellationToken);

        var alreadyExists = members.Any(x => x.ParticipantId == participantId);
        if (alreadyExists)
            return;

        await _epochMemberRepository.AddAsync(
            new EpochMember(
                Guid.NewGuid(),
                epochId,
                participantId,
                DateTime.UtcNow),
            cancellationToken);
    }
}