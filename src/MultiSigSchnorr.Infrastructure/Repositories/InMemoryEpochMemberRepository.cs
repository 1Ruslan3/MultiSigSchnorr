using System.Collections.Concurrent;
using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Entities;

namespace MultiSigSchnorr.Infrastructure.Repositories;

public sealed class InMemoryEpochMemberRepository : IEpochMemberRepository
{
    private readonly ConcurrentDictionary<Guid, EpochMember> _members = new();

    public Task AddAsync(EpochMember member, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(member);

        if (!_members.TryAdd(member.Id, member))
            throw new InvalidOperationException($"Epoch member '{member.Id}' already exists.");

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<EpochMember>> GetByEpochIdAsync(
        Guid epochId,
        CancellationToken cancellationToken = default)
    {
        var result = _members.Values
            .Where(x => x.EpochId == epochId)
            .OrderBy(x => x.AddedUtc)
            .ToList();

        return Task.FromResult<IReadOnlyList<EpochMember>>(result);
    }

    public Task<IReadOnlyList<EpochMember>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EpochMember> result = _members.Values
            .OrderBy(x => x.AddedUtc)
            .ToList();

        return Task.FromResult(result);
    }

    public Task UpdateAsync(EpochMember member, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(member);

        if (!_members.ContainsKey(member.Id))
            throw new InvalidOperationException($"Epoch member '{member.Id}' does not exist.");

        _members[member.Id] = member;
        return Task.CompletedTask;
    }
}