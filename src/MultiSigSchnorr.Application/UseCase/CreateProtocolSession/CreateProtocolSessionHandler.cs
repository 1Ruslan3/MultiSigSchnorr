using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Crypto.Hashing;
using MultiSigSchnorr.Domain.Entities;
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

    public CreateProtocolSessionHandler(
        IEpochRepository epochRepository,
        IParticipantRepository participantRepository,
        IEpochMemberRepository epochMemberRepository,
        IPrivateKeyMaterialRepository privateKeyMaterialRepository,
        IProtocolSessionRepository protocolSessionRepository,
        NPartyCommitmentProtocolService protocolService,
        MessageDigestService messageDigestService)
    {
        _epochRepository = epochRepository ?? throw new ArgumentNullException(nameof(epochRepository));
        _participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
        _epochMemberRepository = epochMemberRepository ?? throw new ArgumentNullException(nameof(epochMemberRepository));
        _privateKeyMaterialRepository = privateKeyMaterialRepository ?? throw new ArgumentNullException(nameof(privateKeyMaterialRepository));
        _protocolSessionRepository = protocolSessionRepository ?? throw new ArgumentNullException(nameof(protocolSessionRepository));
        _protocolService = protocolService ?? throw new ArgumentNullException(nameof(protocolService));
        _messageDigestService = messageDigestService ?? throw new ArgumentNullException(nameof(messageDigestService));
    }

    public async Task<NPartyProtocolSession> HandleAsync(
        CreateProtocolSessionRequest request,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.EpochId == Guid.Empty)
            throw new ArgumentException("Epoch id cannot be empty.", nameof(request));

        if (request.ParticipantIds is null || request.ParticipantIds.Count < 2)
            throw new InvalidOperationException("At least two participants are required.");

        if (string.IsNullOrWhiteSpace(request.Message))
            throw new InvalidOperationException("Message cannot be empty.");

        var epoch = await _epochRepository.GetByIdAsync(request.EpochId, cancellationToken);
        if (epoch is null)
            throw new InvalidOperationException($"Epoch '{request.EpochId}' was not found.");

        var participants = await _participantRepository.GetByIdsAsync(
            request.ParticipantIds,
            cancellationToken);

        if (participants.Count != request.ParticipantIds.Distinct().Count())
            throw new InvalidOperationException("Not all requested participants were found.");

        var epochMembers = await _epochMemberRepository.GetByEpochIdAsync(
            epoch.Id,
            cancellationToken);

        var privateKeys = new Dictionary<Guid, ScalarValue>();

        foreach (var participantId in request.ParticipantIds.Distinct())
        {
            var privateKey = await _privateKeyMaterialRepository.GetByParticipantIdAsync(
                participantId,
                cancellationToken);

            if (privateKey is null)
                throw new InvalidOperationException(
                    $"Private key material for participant '{participantId}' was not found.");

            privateKeys[participantId] = privateKey;
        }

        var digest = _messageDigestService.DigestUtf8(request.Message);

        var session = _protocolService.CreateSession(
            epoch,
            participants,
            epochMembers,
            privateKeys,
            digest,
            nowUtc);

        await _protocolSessionRepository.AddAsync(session, cancellationToken);

        return session;
    }
}