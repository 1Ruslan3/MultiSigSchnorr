using Microsoft.AspNetCore.Mvc;
using MultiSigSchnorr.Api.Development;
using MultiSigSchnorr.Application.Repositories;
using MultiSigSchnorr.Contracts.Diagnostics;

namespace MultiSigSchnorr.Api.Controllers;

[ApiController]
[Route("api/system")]
public sealed class SystemController : ControllerBase
{
    private readonly DevelopmentDataSeeder _developmentDataSeeder;
    private readonly IParticipantRepository _participantRepository;

    public SystemController(
        DevelopmentDataSeeder developmentDataSeeder,
        IParticipantRepository participantRepository)
    {
        _developmentDataSeeder = developmentDataSeeder
            ?? throw new ArgumentNullException(nameof(developmentDataSeeder));
        _participantRepository = participantRepository
            ?? throw new ArgumentNullException(nameof(participantRepository));
    }

    [HttpGet("seed")]
    public async Task<ActionResult<DevelopmentSeedApiResponse>> GetSeed(CancellationToken cancellationToken)
    {
        var snapshot = _developmentDataSeeder.Snapshot;

        if (snapshot is null)
            return NotFound(new { error = "Development seed snapshot is not available." });

        var participants = await _participantRepository.GetByIdsAsync(
            snapshot.ParticipantIds,
            cancellationToken);

        var participantDtos = participants
            .OrderBy(x => x.DisplayName, StringComparer.Ordinal)
            .Select(x => new DevelopmentSeedParticipantApiResponse
            {
                ParticipantId = x.Id,
                DisplayName = x.DisplayName,
                PublicKeyHex = x.PublicKey.ToHex()
            })
            .ToList();

        return Ok(new DevelopmentSeedApiResponse
        {
            EpochId = snapshot.EpochId,
            EpochNumber = snapshot.EpochNumber,
            Participant1Id = snapshot.Participant1Id,
            Participant2Id = snapshot.Participant2Id,
            Participant3Id = snapshot.Participant3Id,
            ParticipantIds = snapshot.ParticipantIds,
            Participants = participantDtos
        });
    }
}