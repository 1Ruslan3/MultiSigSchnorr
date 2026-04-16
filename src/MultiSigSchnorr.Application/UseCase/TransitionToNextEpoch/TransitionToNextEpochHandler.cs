using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Enums;
using MultiSigSchnorr.Protocol.Epochs;

namespace MultiSigSchnorr.Application.UseCases.TransitionToNextEpoch;

public sealed class TransitionToNextEpochHandler
{
    private readonly IEpochRepository _epochRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IEpochMemberRepository _epochMemberRepository;
    private readonly EpochTransitionService _epochTransitionService;

    public TransitionToNextEpochHandler(
        IEpochRepository epochRepository,
        IParticipantRepository participantRepository,
        IEpochMemberRepository epochMemberRepository,
        EpochTransitionService epochTransitionService)
    {
        _epochRepository = epochRepository ?? throw new ArgumentNullException(nameof(epochRepository));
        _participantRepository = participantRepository ?? throw new ArgumentNullException(nameof(participantRepository));
        _epochMemberRepository = epochMemberRepository ?? throw new ArgumentNullException(nameof(epochMemberRepository));
        _epochTransitionService = epochTransitionService ?? throw new ArgumentNullException(nameof(epochTransitionService));
    }

    public async Task HandleAsync(
        TransitionToNextEpochRequest request,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var epochs = await _epochRepository.ListAsync(cancellationToken);
        var activeEpoch = epochs
            .Where(x => x.Status == EpochStatus.Active)
            .OrderByDescending(x => x.Number)
            .FirstOrDefault();

        if (activeEpoch is null)
            throw new InvalidOperationException("Active epoch was not found.");

        var participants = await _participantRepository.ListAsync(cancellationToken);
        var epochMembers = await _epochMemberRepository.GetByEpochIdAsync(activeEpoch.Id, cancellationToken);

        var result = _epochTransitionService.TransitionToNextEpoch(
            activeEpoch,
            participants,
            epochMembers,
            nowUtc);

        await _epochRepository.UpdateAsync(result.PreviousEpoch, cancellationToken);
        await _epochRepository.AddAsync(result.NewEpoch, cancellationToken);

        foreach (var member in result.NewEpochMembers)
            await _epochMemberRepository.AddAsync(member, cancellationToken);
    }
}