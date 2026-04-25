using MultiSigSchnorr.Application.Repositories;

namespace MultiSigSchnorr.Application.UseCases.GetProtocolSessionHistory;

public sealed class GetProtocolSessionHistoryHandler
{
    private readonly IProtocolSessionRepository _protocolSessionRepository;
    private readonly IProtocolSessionProjectionRepository? _projectionRepository;

    public GetProtocolSessionHistoryHandler(
        IProtocolSessionRepository protocolSessionRepository,
        IProtocolSessionProjectionRepository? projectionRepository = null)
    {
        _protocolSessionRepository = protocolSessionRepository
            ?? throw new ArgumentNullException(nameof(protocolSessionRepository));
        _projectionRepository = projectionRepository;
    }

    public async Task<IReadOnlyList<ProtocolSessionHistoryItemDto>> HandleAsync(
        GetProtocolSessionHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var take = request.Take <= 0 ? 20 : Math.Min(request.Take, 100);

        if (_projectionRepository is not null)
        {
            var projections = await _projectionRepository.ListAsync(cancellationToken);

            return projections
                .OrderByDescending(x => x.CreatedUtc)
                .Take(take)
                .Select(x => new ProtocolSessionHistoryItemDto
                {
                    SessionId = x.SessionId,
                    EpochId = x.EpochId,
                    EpochNumber = x.EpochNumber,
                    SessionStatus = x.SessionStatus,
                    ProtectionMode = x.ProtectionMode,
                    CreatedUtc = x.CreatedUtc,
                    CompletedUtc = x.CompletedUtc,
                    ParticipantsCount = x.Participants.Count,
                    AllCommitmentsPublished = x.AllCommitmentsPublished,
                    AllNoncesRevealed = x.AllNoncesRevealed,
                    AllPartialSignaturesSubmitted = x.AllPartialSignaturesSubmitted
                })
                .ToList();
        }

        var sessions = await _protocolSessionRepository.ListAsync(cancellationToken);

        return sessions
            .OrderByDescending(x => x.SigningSession.CreatedUtc)
            .Take(take)
            .Select(x => new ProtocolSessionHistoryItemDto
            {
                SessionId = x.SessionId,
                EpochId = x.Epoch.Id,
                EpochNumber = x.Epoch.Number,
                SessionStatus = x.SigningSession.Status,
                ProtectionMode = x.ProtectionMode,
                CreatedUtc = x.SigningSession.CreatedUtc,
                CompletedUtc = x.SigningSession.CompletedUtc,
                ParticipantsCount = x.Participants.Count,
                AllCommitmentsPublished = x.AllCommitmentsPublished,
                AllNoncesRevealed = x.AllNoncesRevealed,
                AllPartialSignaturesSubmitted = x.AllPartialSignaturesSubmitted
            })
            .ToList();
    }
}