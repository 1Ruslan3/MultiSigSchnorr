using MultiSigSchnorr.Domain.Entities;

namespace MultiSigSchnorr.Application.Repositories;

public interface IParticipantRepository
{
    Task AddAsync(Participant participant, CancellationToken cancellationToken = default);
    Task<Participant?> GetByIdAsync(Guid participantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Participant>> GetByIdsAsync(
        IReadOnlyCollection<Guid> participantIds,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(Participant participant, CancellationToken cancellationToken = default);
}