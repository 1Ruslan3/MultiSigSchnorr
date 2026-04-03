using MultiSigSchnorr.Domain.Entities;

namespace MultiSigSchnorr.Application.Repositories;

public interface ISignatureSessionRepository
{
    Task AddAsync(SignatureSession session, CancellationToken cancellationToken = default);
    Task<SignatureSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task UpdateAsync(SignatureSession session, CancellationToken cancellationToken = default);
}