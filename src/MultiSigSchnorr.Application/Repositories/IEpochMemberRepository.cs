using MultiSigSchnorr.Domain.Entities;

namespace MultiSigSchnorr.Application.Repositories;

public interface IEpochMemberRepository
{
    Task AddAsync(EpochMember member, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EpochMember>> GetByEpochIdAsync(
        Guid epochId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EpochMember>> ListAsync(CancellationToken cancellationToken = default);

    Task UpdateAsync(EpochMember member, CancellationToken cancellationToken = default);
}