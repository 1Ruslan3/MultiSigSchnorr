using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Protocol.Revocation;

namespace MultiSigSchnorr.Application.UseCases.RevokeParticipantInActiveEpoch;

public sealed class RevokeParticipantInActiveEpochHandler
{
    private readonly IEpochRepository _epochRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IEpochMemberRepository _epochMemberRepository;
    private readonly RevocationPolicyService _revocationPolicyService;

    public RevokeParticipantInActiveEpochHandler(
        IEpochRepository epochRepository,
        IParticipantRepository participantRepository,
        IEpochMemberRepository epochMemberRepository,
        RevocationPolicyService revocationPolicyService)
    {
        _epochRepository = epochRepository ?? throw new ArgumentNullException(nameof(epochRepository));
        _participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
        _epochMemberRepository = epochMemberRepository ?? throw new ArgumentNullException(nameof(epochMemberRepository));
        _revocationPolicyService = revocationPolicyService ?? throw new ArgumentNullException(nameof(revocationPolicyService));
    }

    public async Task HandleAsync(
        RevokeParticipantInActiveEpochRequest request,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.ParticipantId == Guid.Empty)
            throw new ArgumentException("Participant id cannot be empty.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new ArgumentException("Revocation reason cannot be empty.", nameof(request));

        var epochs = await _epochRepository.ListAsync(cancellationToken);
        var activeEpoch = epochs
            .Where(x => x.Status == EpochStatus.Active)
            .OrderByDescending(x => x.Number)
            .FirstOrDefault();

        if (activeEpoch is null)
            throw new InvalidOperationException("Active epoch was not found.");

        var participant = await _participantRepository.GetByIdAsync(request.ParticipantId, cancellationToken);
        if (participant is null)
            throw new InvalidOperationException($"Participant '{request.ParticipantId}' was not found.");

        var epochMembers = await _epochMemberRepository.GetByEpochIdAsync(activeEpoch.Id, cancellationToken);

        var result = _revocationPolicyService.RevokeParticipant(
            activeEpoch,
            participant,
            epochMembers,
            request.Reason,
            nowUtc);

        await _participantRepository.UpdateAsync(result.Participant, cancellationToken);

        foreach (var member in result.DeactivatedMembers)
            await _epochMemberRepository.UpdateAsync(member, cancellationToken);
    }
}