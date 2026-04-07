using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Entities;
using MultiSigSchnorr.Domain.Enums;

namespace MultiSigSchnorr.Application.UseCases.CreateSignatureSession;

public sealed class CreateSignatureSessionHandler
{
    private readonly IEpochRepository _epochRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IEpochMemberRepository _epochMemberRepository;
    private readonly ISignatureSessionRepository _signatureSessionRepository;

    public CreateSignatureSessionHandler(
        IEpochRepository epochRepository,
        IParticipantRepository participantRepository,
        IEpochMemberRepository epochMemberRepository,
        ISignatureSessionRepository signatureSessionRepository)
    {
        _epochRepository = epochRepository ?? throw new ArgumentNullException(nameof(epochRepository));
        _participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
        _epochMemberRepository = epochMemberRepository ?? throw new ArgumentNullException(nameof(epochMemberRepository));
        _signatureSessionRepository = signatureSessionRepository ?? throw new ArgumentNullException(nameof(signatureSessionRepository));
    }

    public async Task<SignatureSession> HandleAsync(
        CreateSignatureSessionRequest request,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.EpochId == Guid.Empty)
            throw new ArgumentException("Request epoch id cannot be empty.", nameof(request));

        if (request.ParticipantIds is null || request.ParticipantIds.Count < 2)
            throw new InvalidOperationException("At least two participants are required to create a signing session.");

        if (request.Message is null || request.Message.Length == 0)
            throw new InvalidOperationException("Message cannot be empty.");

        var epoch = await _epochRepository.GetByIdAsync(request.EpochId, cancellationToken);
        if (epoch is null)
            throw new InvalidOperationException($"Epoch '{request.EpochId}' was not found.");

        if (epoch.Status != EpochStatus.Active)
            throw new InvalidOperationException("Epoch must be active.");

        var participants = await _participantRepository.GetByIdsAsync(
            request.ParticipantIds,
            cancellationToken);

        var participantMap = participants
            .GroupBy(x => x.Id)
            .ToDictionary(g => g.Key, g => g.First());

        var epochMembers = await _epochMemberRepository.GetByEpochIdAsync(epoch.Id, cancellationToken);

        var activeMemberIds = epochMembers
            .Where(x => x.IsActive)
            .Select(x => x.ParticipantId)
            .ToHashSet();

        foreach (var participantId in request.ParticipantIds.Distinct())
        {
            if (!activeMemberIds.Contains(participantId))
                throw new InvalidOperationException(
                    $"Participant '{participantId}' is not an active member of epoch '{epoch.Id}'.");

            if (!participantMap.TryGetValue(participantId, out var participant))
                throw new InvalidOperationException(
                    $"Participant '{participantId}' was not found in the participant repository.");

            if (participant.Status != ParticipantStatus.Active)
                throw new InvalidOperationException(
                    $"Participant '{participantId}' is not active.");
        }

        var session = new SignatureSession(
            Guid.NewGuid(),
            epoch.Id,
            request.ParticipantIds,
            request.Message,
            nowUtc);

        await _signatureSessionRepository.AddAsync(session, cancellationToken);

        return session;
    }
}