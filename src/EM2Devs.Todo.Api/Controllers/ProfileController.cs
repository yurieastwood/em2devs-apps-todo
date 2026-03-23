using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;

namespace EM2Devs.Todo.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/profile")]
[Route("api/v{version:apiVersion}/profile")]
public sealed class ProfileController : ControllerBase
{
    private readonly IPlayerProfileRepository _profileRepository;

    public ProfileController(IPlayerProfileRepository profileRepository) =>
        _profileRepository = profileRepository;

    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        PlayerProfile profile = await _profileRepository.GetProfileAsync(ct).ConfigureAwait(false);

        XpBreakdownResponse? breakdown = profile.LastXpBreakdown is not null
            ? new XpBreakdownResponse(
                profile.LastXpBreakdown.BaseXp,
                profile.LastXpBreakdown.DeadlineModifier,
                profile.LastXpBreakdown.StreakMultiplier,
                profile.LastXpBreakdown.FinalXp)
            : null;

        return Ok(new ProfileResponse(
            profile.TotalXp,
            profile.Level,
            profile.XpToNextLevel,
            profile.CurrentStreak,
            profile.LongestStreak,
            breakdown));
    }
}

public sealed record XpBreakdownResponse(
    int BaseXp,
    double DeadlineModifier,
    double StreakMultiplier,
    int FinalXp);

public sealed record ProfileResponse(
    int TotalXp,
    int Level,
    int XpToNextLevel,
    int CurrentStreak,
    int LongestStreak,
    XpBreakdownResponse? LastXpBreakdown);
