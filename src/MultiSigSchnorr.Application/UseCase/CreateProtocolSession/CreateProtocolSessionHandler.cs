using MultiSigSchnorr.Application.Audit;
using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Crypto.Hashing;
using MultiSigSchnorr.Domain.ValueObjects;
using MultiSigSchnorr.Protocol.Models;
using MultiSigSchnorr.Protocol.Sessions;

namespace MultiSigSchnorr.Application.UseCases.CreateProtocolSession;

public sealed class CreateProtocolSessionHandler
{
    private readonly IEpochRepository _epochRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IEpochMemberRepository _epochMemberRepository;
    private readonly IPrivateKeyMaterialRepository _privateKeyMaterialRepository;
    private readonly IProtocolSessionRepository _protocolSessionRepository;
    private readonly NPartyCommitmentProtocolService _protocolService;
    private readonly MessageDigestService _messageDigestService;
    private readonly AuditLogService _auditLogService;
    private readonly IProtocolSessionProjectionRepository? _projectionRepository;

    public CreateProtocolSessionHandler(
        IEpochRepository epochRepository,
        IParticipantRepository participantRepository,
        IEpochMemberRepository epochMemberRepository,
        IPrivateKeyMaterialRepository privateKeyMaterialRepository,
        IProtocolSessionRepository protocolSessionRepository,
        NPartyCommitmentProtocolService protocolService,
        MessageDigestService messageDigestService,
        AuditLogService auditLogService,
        IProtocolSessionProjectionRepository? projectionRepository = null)
    {
        _epochRepository = epochRepository ?? throw new ArgumentNullException(nameof(epochRepository));
        _participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
        _epochMemberRepository = epochMemberRepository ?? throw new ArgumentNullException(nameof(epochMemberRepository));
        _privateKeyMaterialRepository = privateKeyMaterialRepository ?? throw new ArgumentNullException(nameof(privateKeyMaterialRepository));
        _protocolSessionRepository = protocolSessionRepository ?? throw new ArgumentNullException(nameof(protocolSessionRepository));
        _protocolService = protocolService ?? throw new ArgumentNullException(nameof(protocolService));
        _messageDigestService = messageDigestService ?? throw new ArgumentNullException(nameof(messageDigestService));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        _projectionRepository = projectionRepository;
    }

    public async Task<NPartyProtocolSession> HandleAsync(
        CreateProtocolSessionRequest request,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.EpochId == Guid.Empty)
            throw new ArgumentException("Epoch id cannot be empty.", nameof(request));

        if (request.ParticipantIds.Count < 2)
            throw new InvalidOperationException("At least two participants are required.");

        if (string.IsNullOrWhiteSpace(request.Message))
            throw new ArgumentException("Message cannot be empty.", nameof(request));

        var epoch = await _epochRepository.GetByIdAsync(request.EpochId, cancellationToken);
        if (epoch is null)
            throw new InvalidOperationException($"Epoch '{request.EpochId}' was not found.");

        var participants = await _participantRepository.GetByIdsAsync(request.ParticipantIds, cancellationToken);

        if (participants.Count != request.ParticipantIds.Distinct().Count())
            throw new InvalidOperationException("One or more participants were not found.");

        var epochMembers = await _epochMemberRepository.GetByEpochIdAsync(epoch.Id, cancellationToken);

        var privateKeys = new Dictionary<Guid, ScalarValue>();

        foreach (var participantId in request.ParticipantIds.Distinct())
        {
            var privateKey = await _privateKeyMaterialRepository.GetByParticipantIdAsync(
                participantId,
                cancellationToken);

            if (privateKey is null)
                throw new InvalidOperationException($"Private key material for participant '{participantId}' was not found.");

            privateKeys[participantId] = privateKey;
        }

        var messageDigest = _messageDigestService.DigestUtf8(request.Message);

        var session = _protocolService.CreateSession(
            epoch,
            participants,
            epochMembers,
            privateKeys,
            messageDigest,
            nowUtc,
            request.ProtectionMode);

        await _protocolSessionRepository.AddAsync(session, cancellationToken);

        if (_projectionRepository is not null)
            await _projectionRepository.UpsertAsync(session, cancellationToken);

        await _auditLogService.LogProtocolSessionCreatedAsync(
            session.SessionId,
            epoch.Id,
            epoch.Number,
            request.ProtectionMode,
            request.ParticipantIds.Distinct().ToList(),
            nowUtc,
            cancellationToken);

        return session;
    }
}