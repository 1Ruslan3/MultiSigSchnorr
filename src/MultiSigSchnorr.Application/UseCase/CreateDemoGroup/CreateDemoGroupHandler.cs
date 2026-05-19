using System.Security.Cryptography;
using MultiSigSchnorr.Application.Audit;
using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Crypto.Curves;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Application.UseCases.CreateDemoGroup;

public sealed class CreateDemoGroupHandler
{
    private const int MinimumParticipants = 2;
    private const int MaximumParticipants = 10;

    private readonly IEpochRepository _epochRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IEpochMemberRepository _epochMemberRepository;
    private readonly IPrivateKeyMaterialRepository _privateKeyMaterialRepository;
    private readonly PublicKeyGenerationService _publicKeyGenerationService;
    private readonly P256CurveContext _curve;
    private readonly AuditLogService _auditLogService;

    public CreateDemoGroupHandler(
        IEpochRepository epochRepository,
        IParticipantRepository participantRepository,
        IEpochMemberRepository epochMemberRepository,
        IPrivateKeyMaterialRepository privateKeyMaterialRepository,
        PublicKeyGenerationService publicKeyGenerationService,
        P256CurveContext curve,
        AuditLogService auditLogService)
    {
        _epochRepository = epochRepository ?? throw new ArgumentNullException(nameof(epochRepository));
        _participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
        _epochMemberRepository = epochMemberRepository ?? throw new ArgumentNullException(nameof(epochMemberRepository));
        _privateKeyMaterialRepository = privateKeyMaterialRepository ?? throw new ArgumentNullException(nameof(privateKeyMaterialRepository));
        _publicKeyGenerationService = publicKeyGenerationService ?? throw new ArgumentNullException(nameof(publicKeyGenerationService));
        _curve = curve ?? throw new ArgumentNullException(nameof(curve));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
    }

    public async Task<Epoch> HandleAsync(
        CreateDemoGroupRequest request,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.ParticipantsCount < MinimumParticipants ||
            request.ParticipantsCount > MaximumParticipants)
        {
            throw new InvalidOperationException(
                $"Demo group participants count must be between {MinimumParticipants} and {MaximumParticipants}.");
        }

        var prefix = string.IsNullOrWhiteSpace(request.DisplayNamePrefix)
            ? "Demo Signer"
            : request.DisplayNamePrefix.Trim();

        var suffix = nowUtc.ToString("yyyyMMddHHmmss");
        var participants = new List<Participant>(request.ParticipantsCount);

        for (var index = 1; index <= request.ParticipantsCount; index++)
        {
            var privateKey = GeneratePrivateKey();
            var publicKey = _publicKeyGenerationService.DerivePublicKey(privateKey);

            var participant = new Participant(
                Guid.NewGuid(),
                $"{prefix}-{suffix}-{index}",
                publicKey,
                ParticipantStatus.Active,
                nowUtc);

            await _participantRepository.AddAsync(participant, cancellationToken);
            await _privateKeyMaterialRepository.SetAsync(participant.Id, privateKey, cancellationToken);

            await _auditLogService.LogParticipantCreatedAsync(
                participant.Id,
                participant.DisplayName,
                participant.PublicKey.ToHex(),
                nowUtc,
                cancellationToken);

            participants.Add(participant);
        }

        var epochs = await _epochRepository.ListAsync(cancellationToken);
        var activeEpochs = epochs
            .Where(x => x.Status == EpochStatus.Active)
            .ToList();

        var closedEpochIds = activeEpochs
            .Select(x => x.Id)
            .ToList();

        foreach (var activeEpoch in activeEpochs)
        {
            activeEpoch.Close(nowUtc);
            await _epochRepository.UpdateAsync(activeEpoch, cancellationToken);
        }

        var nextEpochNumber = epochs.Count == 0
            ? 1
            : epochs.Max(x => x.Number) + 1;

        var epoch = new Epoch(
            Guid.NewGuid(),
            nextEpochNumber,
            nowUtc);

        epoch.Activate(nowUtc);

        await _epochRepository.AddAsync(epoch, cancellationToken);

        foreach (var participant in participants)
        {
            await _epochMemberRepository.AddAsync(
                new EpochMember(
                    Guid.NewGuid(),
                    epoch.Id,
                    participant.Id,
                    nowUtc),
                cancellationToken);
        }

        var participantIds = participants.Select(x => x.Id).ToList();

        await _auditLogService.LogEpochCreatedWithMembersAsync(
            epoch.Id,
            epoch.Number,
            closedEpochIds,
            participantIds,
            nowUtc,
            cancellationToken);

        await _auditLogService.LogDemoGroupCreatedAsync(
            epoch.Id,
            epoch.Number,
            prefix,
            participantIds,
            nowUtc,
            cancellationToken);

        return epoch;
    }

    private ScalarValue GeneratePrivateKey()
    {
        for (var attempt = 0; attempt < 16; attempt++)
        {
            var candidate = _curve.ReduceScalar(RandomNumberGenerator.GetBytes(32));

            if (!string.IsNullOrWhiteSpace(candidate.ToHex()) &&
                candidate.ToHex().Trim('0').Length > 0)
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("Failed to generate non-zero private key material.");
    }
}
