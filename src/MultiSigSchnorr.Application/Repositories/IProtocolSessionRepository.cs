using MultiSigSchnorr.Protocol.Models;

namespace MultiSigSchnorr.Application.Repositories;

public interface IProtocolSessionRepository
{
    Task AddAsync(NPartyProtocolSession session, CancellationToken cancellationToken = default);

    Task<NPartyProtocolSession?> GetByIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(NPartyProtocolSession session, CancellationToken cancellationToken = default);
}