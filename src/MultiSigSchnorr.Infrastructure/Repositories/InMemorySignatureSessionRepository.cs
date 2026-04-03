using System.Collections.Concurrent;
using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Entities;

namespace MultiSigSchnorr.Infrastructure.Repositories;

public sealed class InMemorySignatureSessionRepository : ISignatureSessionRepository
{
    private readonly ConcurrentDictionary<Guid, SignatureSession> _sessions = new();

    public Task AddAsync(SignatureSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (!_sessions.TryAdd(session.Id, session))
            throw new InvalidOperationException($"Signature session '{session.Id}' already exists.");

        return Task.CompletedTask;
    }

    public Task<SignatureSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return Task.FromResult(session);
    }

    public Task UpdateAsync(SignatureSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (!_sessions.ContainsKey(session.Id))
            throw new InvalidOperationException($"Signature session '{session.Id}' does not exist.");

        _sessions[session.Id] = session;
        return Task.CompletedTask;
    }
}