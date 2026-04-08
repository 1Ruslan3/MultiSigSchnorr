using System.Collections.Concurrent;
using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Infrastructure.Repositories;

public sealed class InMemoryPrivateKeyMaterialRepository : IPrivateKeyMaterialRepository
{
    private readonly ConcurrentDictionary<Guid, ScalarValue> _keys = new();

    public Task SetAsync(
        Guid participantId,
        ScalarValue privateKey,
        CancellationToken cancellationToken = default)
    {
        if (participantId == Guid.Empty)
            throw new ArgumentException("Participant id cannot be empty.", nameof(participantId));

        ArgumentNullException.ThrowIfNull(privateKey);

        _keys[participantId] = privateKey;
        return Task.CompletedTask;
    }

    public Task<ScalarValue?> GetByParticipantIdAsync(
        Guid participantId,
        CancellationToken cancellationToken = default)
    {
        _keys.TryGetValue(participantId, out var privateKey);
        return Task.FromResult(privateKey);
    }
}