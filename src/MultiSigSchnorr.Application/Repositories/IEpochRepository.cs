using MultiSigSchnorr.Domain.Entities;

namespace MultiSigSchnorr.Application.Repositories;

public interface IEpochRepository
{
    Task AddAsync(Epoch epoch, CancellationToken cancellationToken = default);
    Task<Epoch?> GetByIdAsync(Guid epochId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Epoch epoch, CancellationToken cancellationToken = default);
}