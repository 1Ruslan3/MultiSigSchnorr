using Microsoft.AspNetCore.Mvc;
using MultiSigSchnorr.Api.Development;
using MultiSigSchnorr.Contracts.Diagnostics;

namespace MultiSigSchnorr.Api.Controllers;

[ApiController]
[Route("api/system")]
public sealed class SystemController : ControllerBase
{
    private readonly DevelopmentDataSeeder _developmentDataSeeder;

    public SystemController(DevelopmentDataSeeder developmentDataSeeder)
    {
        _developmentDataSeeder = developmentDataSeeder
            ?? throw new ArgumentNullException(nameof(developmentDataSeeder));
    }

    [HttpGet("seed")]
    public ActionResult<DevelopmentSeedApiResponse> GetSeed()
    {
        var snapshot = _developmentDataSeeder.Snapshot;

        if (snapshot is null)
            return NotFound(new { error = "Development seed snapshot is not available." });

        return Ok(new DevelopmentSeedApiResponse
        {
            EpochId = snapshot.EpochId,
            EpochNumber = snapshot.EpochNumber,
            Participant1Id = snapshot.Participant1Id,
            Participant2Id = snapshot.Participant2Id,
            Participant3Id = snapshot.Participant3Id,
            ParticipantIds = snapshot.ParticipantIds
        });
    }
}