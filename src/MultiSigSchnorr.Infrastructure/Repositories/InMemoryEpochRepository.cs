using System.Collections.Concurrent;
using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Entities;

namespace MultiSigSchnorr.Infrastructure.Repositories;

public sealed class InMemoryEpochRepository : IEpochRepository
{
    private readonly ConcurrentDictionary<Guid, Epoch> _epochs = new();

    public Task AddAsync(Epoch epoch, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(epoch);

        if (!_epochs.TryAdd(epoch.Id, epoch))
            throw new InvalidOperationException($"Epoch '{epoch.Id}' already exists.");

        return Task.CompletedTask;
    }

    public Task<Epoch?> GetByIdAsync(Guid epochId, CancellationToken cancellationToken = default)
    {
        _epochs.TryGetValue(epochId, out var epoch);
        return Task.FromResult(epoch);
    }

    public Task<IReadOnlyList<Epoch>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Epoch> result = _epochs.Values
            .OrderByDescending(x => x.Number)
            .ToList();

        return Task.FromResult(result);
    }

    public Task UpdateAsync(Epoch epoch, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(epoch);

        if (!_epochs.ContainsKey(epoch.Id))
            throw new InvalidOperationException($"Epoch '{epoch.Id}' does not exist.");

        _epochs[epoch.Id] = epoch;
        return Task.CompletedTask;
    }
}