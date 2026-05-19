using System.Security.Cryptography;
using MultiSigSchnorr.Application.Audit;
using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Crypto.Curves;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Application.UseCases.CreateParticipant;

public sealed class CreateParticipantHandler
{
    private readonly IParticipantRepository _participantRepository;
    private readonly IPrivateKeyMaterialRepository _privateKeyMaterialRepository;
    private readonly PublicKeyGenerationService _publicKeyGenerationService;
    private readonly P256CurveContext _curve;
    private readonly AuditLogService _auditLogService;

    public CreateParticipantHandler(
        IParticipantRepository participantRepository,
        IPrivateKeyMaterialRepository privateKeyMaterialRepository,
        PublicKeyGenerationService publicKeyGenerationService,
        P256CurveContext curve,
        AuditLogService auditLogService)
    {
        _participantRepository = participantRepository
            ?? throw new ArgumentNullException(nameof(participantRepository));
        _privateKeyMaterialRepository = privateKeyMaterialRepository
            ?? throw new ArgumentNullException(nameof(privateKeyMaterialRepository));
        _publicKeyGenerationService = publicKeyGenerationService
            ?? throw new ArgumentNullException(nameof(publicKeyGenerationService));
        _curve = curve ?? throw new ArgumentNullException(nameof(curve));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
    }

    public async Task<Participant> HandleAsync(
        CreateParticipantRequest request,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.DisplayName))
            throw new ArgumentException("Display name cannot be empty.", nameof(request));

        var privateKey = GeneratePrivateKey();
        var publicKey = _publicKeyGenerationService.DerivePublicKey(privateKey);

        var participant = new Participant(
            Guid.NewGuid(),
            request.DisplayName,
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

        return participant;
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
