using MultiSigSchnorr.Domain.ValueObjects;

namespace MultiSigSchnorr.Application.Repositories;

public interface IPrivateKeyMaterialRepository
{
    Task SetAsync(
        Guid participantId,
        ScalarValue privateKey,
        CancellationToken cancellationToken = default);

    Task<ScalarValue?> GetByParticipantIdAsync(
        Guid participantId,
        CancellationToken cancellationToken = default);

    Task<bool> HasPrivateKeyMaterialAsync(
        Guid participantId,
        CancellationToken cancellationToken = default);
}
