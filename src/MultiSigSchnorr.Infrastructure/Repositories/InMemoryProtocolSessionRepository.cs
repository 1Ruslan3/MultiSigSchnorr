using System.Collections.Concurrent;
using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Protocol.Models;

namespace MultiSigSchnorr.Infrastructure.Repositories;

public sealed class InMemoryProtocolSessionRepository : IProtocolSessionRepository
{
    private readonly ConcurrentDictionary<Guid, NPartyProtocolSession> _sessions = new();

    public Task AddAsync(NPartyProtocolSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (!_sessions.TryAdd(session.SessionId, session))
            throw new InvalidOperationException($"Protocol session '{session.SessionId}' already exists.");

        return Task.CompletedTask;
    }

    public Task<NPartyProtocolSession?> GetByIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return Task.FromResult(session);
    }

    public Task<IReadOnlyList<NPartyProtocolSession>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<NPartyProtocolSession> result = _sessions.Values.ToList();
        return Task.FromResult(result);
    }

    public Task UpdateAsync(NPartyProtocolSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (!_sessions.ContainsKey(session.SessionId))
            throw new InvalidOperationException($"Protocol session '{session.SessionId}' does not exist.");

        _sessions[session.SessionId] = session;
        return Task.CompletedTask;
    }
}