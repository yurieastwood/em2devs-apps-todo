using Microsoft.AspNetCore.Mvc;
using EM2Devs.Todo.Application.Ports;

namespace EM2Devs.Todo.Api.Controllers;

[ApiController]
[Route("api/profile")]
public sealed class ProfileController : ControllerBase
{
    private readonly IPlayerProfileRepository _profileRepository;

    public ProfileController(IPlayerProfileRepository profileRepository) =>
        _profileRepository = profileRepository;

    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var profile = await _profileRepository.GetProfileAsync(ct).ConfigureAwait(false);

        return Ok(new ProfileResponse(
            profile.TotalXp,
            profile.Level,
            profile.XpToNextLevel,
            profile.CurrentStreak,
            profile.LongestStreak));
    }
}

public sealed record ProfileResponse(
    int TotalXp,
    int Level,
    int XpToNextLevel,
    int CurrentStreak,
    int LongestStreak);
