using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Domain.Entities;

namespace MultiSigSchnorr.Application.UseCases.RenameParticipant;

public sealed class RenameParticipantHandler
{
    private readonly IParticipantRepository _participantRepository;

    public RenameParticipantHandler(IParticipantRepository participantRepository)
    {
        _participantRepository = participantRepository
            ?? throw new ArgumentNullException(nameof(participantRepository));
    }

    public async Task<Participant> HandleAsync(
        RenameParticipantRequest request,
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

        participant.Rename(request.DisplayName);

        await _participantRepository.UpdateAsync(participant, cancellationToken);

        return participant;
    }
}