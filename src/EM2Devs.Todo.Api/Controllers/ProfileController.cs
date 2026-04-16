using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EM2Devs.Todo.Api.Extensions;
using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;

namespace EM2Devs.Todo.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/profile")]
[Route("api/v{version:apiVersion}/profile")]
public sealed class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator) =>
        _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        Result<PlayerProfileReadModel> result =
            await _mediator.Send(new GetPlayerProfileQuery(), ct).ConfigureAwait(false);

        return result.Match<IActionResult>(
            profile => Ok(Map(profile)),
            error => Problem(error.Message, statusCode: 500));
    }

    /// <summary>
    /// Returns the authenticated user's estimation calibration. Bias factor is a
    /// multiplier against the raw estimate: &gt; 1.0 means the user underestimates;
    /// &lt; 1.0 means the user overestimates. When the user has too few completed
    /// tasks with actual times recorded, <c>calibrationState</c> is
    /// <c>NotEnoughData</c> and the factor is neutral (1.0).
    /// </summary>
    [HttpGet("estimation-bias")]
    public async Task<IActionResult> GetEstimationBias(CancellationToken ct)
    {
        Result<EstimationCalibrationReadModel> result =
            await _mediator.Send(new GetEstimationBiasQuery(), ct).ConfigureAwait(false);

        return result.Match<IActionResult>(
            calibration => Ok(new EstimationBiasResponse(
                calibration.BiasFactor,
                calibration.SampleSize,
                calibration.CalibrationState)),
            error => Problem(error.Message, statusCode: 500));
    }

    [HttpGet("estimation-accuracy")]
    public async Task<IActionResult> GetEstimationAccuracy(CancellationToken ct)
    {
        Result<EstimationDashboardReadModel> result =
            await _mediator.Send(new GetEstimationDashboardQuery(), ct).ConfigureAwait(false);

        return result.Match<IActionResult>(
            dashboard => Ok(dashboard),
            error => Problem(error.Message, statusCode: 500));
    }

    /// <summary>
    /// Activates a streak freeze for the authenticated user. Returns the updated
    /// profile with the active freeze populated, or 409 if a freeze is already active.
    /// </summary>
    [HttpPost("streak/freeze")]
    public async Task<IActionResult> FreezeStreak(
        [FromBody] FreezeStreakRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Result<PlayerProfileReadModel> result =
            await _mediator.Send(new FreezeStreakCommand(request.Days), ct).ConfigureAwait(false);

        return result.ToHttpResult(profile => Ok(Map(profile)));
    }

    private static ProfileResponse Map(PlayerProfileReadModel profile)
    {
        XpBreakdownResponse? breakdown = profile.LastXpBreakdown is not null
            ? new XpBreakdownResponse(
                profile.LastXpBreakdown.BaseXp,
                profile.LastXpBreakdown.DeadlineModifier,
                profile.LastXpBreakdown.StreakMultiplier,
                profile.LastXpBreakdown.FinalXp)
            : null;

        IReadOnlyList<XpHistoryEntryResponse> xpHistory = (profile.XpHistory ?? [])
            .Select(e => new XpHistoryEntryResponse(e.Date, e.XpEarned, e.Source, e.CumulativeTotal))
            .ToList();

        TitlesResponse titles = profile.Titles is { } t
            ? new TitlesResponse(
                t.Earned.Select(tt => new TitleResponse(tt.Type, tt.DisplayName, tt.EarnedOn)).ToList(),
                t.Active,
                t.Progress.Select(p => new TitleProgressResponse(p.Type, p.ProgressPercentage, p.RemainingDescription)).ToList())
            : new TitlesResponse([], null, []);

        IReadOnlyList<SkillTreeResponse> skillTrees = (profile.SkillTrees ?? [])
            .Select(s => new SkillTreeResponse(
                s.Type,
                s.Tier,
                s.TasksCompletedInTier,
                s.TasksToNextTier,
                s.UnlockHint,
                s.Perks.Select(p => new SkillTreePerkResponse(p.Tier, p.PerkType, p.Description)).ToList()))
            .ToList();

        StreakFreezeResponse? streakFreeze = profile.StreakFreeze is { } f
            ? new StreakFreezeResponse(f.FrozenAt, f.Days, f.ExpiresAt)
            : null;

        return new ProfileResponse(
            profile.TotalXp,
            profile.Level,
            profile.XpToNextLevel,
            profile.XpProgressPercent,
            profile.XpThisWeek,
            profile.XpThisSeason,
            profile.CurrentStreak,
            profile.LongestStreak,
            breakdown,
            xpHistory,
            titles,
            skillTrees,
            streakFreeze);
    }
}

public sealed record FreezeStreakRequest(int Days);

public sealed record EstimationBiasResponse(
    double BiasFactor,
    int SampleSize,
    string CalibrationState);

public sealed record StreakFreezeResponse(
    DateOnly FrozenAt,
    int Days,
    DateOnly ExpiresAt);

public sealed record XpBreakdownResponse(
    int BaseXp,
    double DeadlineModifier,
    double StreakMultiplier,
    int FinalXp);

public sealed record XpHistoryEntryResponse(
    DateOnly Date,
    int XpEarned,
    string Source,
    int CumulativeTotal);

public sealed record TitleResponse(
    string Type,
    string DisplayName,
    DateOnly EarnedOn);

public sealed record TitleProgressResponse(
    string Type,
    int ProgressPercentage,
    string RemainingDescription);

public sealed record TitlesResponse(
    IReadOnlyList<TitleResponse> Earned,
    string? Active,
    IReadOnlyList<TitleProgressResponse> Progress);

public sealed record SkillTreePerkResponse(
    int Tier,
    string PerkType,
    string Description);

public sealed record SkillTreeResponse(
    string Type,
    int? Tier,
    int? TasksCompletedInTier,
    int? TasksToNextTier,
    string? UnlockHint,
    IReadOnlyList<SkillTreePerkResponse> Perks);

public sealed record ProfileResponse(
    int TotalXp,
    int Level,
    int XpToNextLevel,
    int XpProgressPercent,
    int XpThisWeek,
    int XpThisSeason,
    int CurrentStreak,
    int LongestStreak,
    XpBreakdownResponse? LastXpBreakdown,
    IReadOnlyList<XpHistoryEntryResponse> XpHistory,
    TitlesResponse Titles,
    IReadOnlyList<SkillTreeResponse> SkillTrees,
    StreakFreezeResponse? StreakFreeze);
