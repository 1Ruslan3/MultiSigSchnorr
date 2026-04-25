using MultiSigSchnorr.Application.Projections;
using MultiSigSchnorr.Protocol.Models;

namespace MultiSigSchnorr.Application.Repositories;

public interface IProtocolSessionProjectionRepository
{
    Task UpsertAsync(
        NPartyProtocolSession session,
        CancellationToken cancellationToken = default);

    Task<ProtocolSessionProjection?> GetByIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProtocolSessionProjection>> ListAsync(
        CancellationToken cancellationToken = default);
}