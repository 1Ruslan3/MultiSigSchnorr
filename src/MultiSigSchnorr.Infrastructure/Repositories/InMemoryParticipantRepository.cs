using System.Collections.Concurrent;
using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Entities;

namespace MultiSigSchnorr.Infrastructure.Repositories;

public sealed class InMemoryParticipantRepository : IParticipantRepository
{
    private readonly ConcurrentDictionary<Guid, Participant> _participants = new();

    public Task AddAsync(Participant participant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(participant);

        if (!_participants.TryAdd(participant.Id, participant))
            throw new InvalidOperationException($"Participant '{participant.Id}' already exists.");

        return Task.CompletedTask;
    }

    public Task<Participant?> GetByIdAsync(Guid participantId, CancellationToken cancellationToken = default)
    {
        _participants.TryGetValue(participantId, out var participant);
        return Task.FromResult(participant);
    }

    public Task<IReadOnlyList<Participant>> GetByIdsAsync(
        IReadOnlyCollection<Guid> participantIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(participantIds);

        var result = participantIds
            .Distinct()
            .Where(_participants.ContainsKey)
            .Select(id => _participants[id])
            .ToList();

        return Task.FromResult<IReadOnlyList<Participant>>(result);
    }

    public Task<IReadOnlyList<Participant>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Participant> result = _participants.Values
            .OrderBy(x => x.DisplayName, StringComparer.Ordinal)
            .ToList();

        return Task.FromResult(result);
    }

    public Task UpdateAsync(Participant participant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(participant);

        if (!_participants.ContainsKey(participant.Id))
            throw new InvalidOperationException($"Participant '{participant.Id}' does not exist.");

        _participants[participant.Id] = participant;
        return Task.CompletedTask;
    }
}