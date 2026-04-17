using MultiSigSchnorr.Application.Repositories;

namespace MultiSigSchnorr.Application.UseCases.GetProtocolSessionHistory;

public sealed class GetProtocolSessionHistoryHandler
{
    private readonly IProtocolSessionRepository _protocolSessionRepository;

    public GetProtocolSessionHistoryHandler(IProtocolSessionRepository protocolSessionRepository)
    {
        _protocolSessionRepository = protocolSessionRepository
            ?? throw new ArgumentNullException(nameof(protocolSessionRepository));
    }

    public async Task<IReadOnlyList<ProtocolSessionHistoryItemDto>> HandleAsync(
        GetProtocolSessionHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var take = request.Take <= 0 ? 20 : Math.Min(request.Take, 100);

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