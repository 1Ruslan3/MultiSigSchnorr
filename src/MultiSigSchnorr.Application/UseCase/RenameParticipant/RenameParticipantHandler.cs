using MultiSigSchnorr.Application.Audit;
using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Entities;

namespace MultiSigSchnorr.Application.UseCases.RenameParticipant;

public sealed class RenameParticipantHandler
{
    private readonly IParticipantRepository _participantRepository;
    private readonly AuditLogService _auditLogService;

    public RenameParticipantHandler(
        IParticipantRepository participantRepository,
        AuditLogService auditLogService)
    {
        _participantRepository = participantRepository
            ?? throw new ArgumentNullException(nameof(participantRepository));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
    }

    public async Task<Participant> HandleAsync(
        RenameParticipantRequest request,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.ParticipantId == Guid.Empty)
            throw new ArgumentException("Participant id cannot be empty.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.DisplayName))
            throw new ArgumentException("Display name cannot be empty.", nameof(request));

        var participant = await _participantRepository.GetByIdAsync(
            request.ParticipantId,
            cancellationToken);

        if (participant is null)
            throw new InvalidOperationException($"Participant '{request.ParticipantId}' was not found.");

        var oldDisplayName = participant.DisplayName;

        participant.Rename(request.DisplayName);

        await _participantRepository.UpdateAsync(participant, cancellationToken);

        await _auditLogService.LogParticipantRenamedAsync(
            participant.Id,
            oldDisplayName,
            participant.DisplayName,
            nowUtc,
            cancellationToken);

        return participant;
    }
}
