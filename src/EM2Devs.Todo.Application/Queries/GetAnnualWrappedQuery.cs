using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Queries;

public sealed record GetAnnualWrappedQuery(int? Year = null) : IRequest<Result<AnnualWrappedReadModel>>;

public sealed class GetAnnualWrappedQueryHandler
    : IRequestHandler<GetAnnualWrappedQuery, Result<AnnualWrappedReadModel>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IPlayerProfileRepository _profileRepository;
    private readonly TimeProvider _timeProvider;

    public GetAnnualWrappedQueryHandler(
        ITaskRepository taskRepository,
        IPlayerProfileRepository profileRepository,
        TimeProvider timeProvider)
    {
        _taskRepository = taskRepository;
        _profileRepository = profileRepository;
        _timeProvider = timeProvider;
    }

    public async Task<Result<AnnualWrappedReadModel>> Handle(GetAnnualWrappedQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        DateOnly today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        int year = request.Year ?? today.Year;

        IReadOnlyList<TodoTask> tasks = await _taskRepository.GetAllAsync(ct).ConfigureAwait(false);
        PlayerProfileReadModel profile = await _profileRepository.GetProfileAsync(ct).ConfigureAwait(false);

        var yearTasks = tasks
            .Where(t => t.CompletedAt.HasValue && t.CompletedAt.Value.Year == year)
            .ToList();

        var slides = new List<WrappedSlide>();

        int totalCompleted = yearTasks.Count;
        slides.Add(totalCompleted > 0
            ? new WrappedSlide("Tasks Completed", $"{totalCompleted}", "counter")
            : WrappedSlide.CreateEncouraging("Tasks Completed", "Your first completed tasks await next year!", "counter"));

        slides.Add(new WrappedSlide("Total XP Earned", $"{profile.TotalXp}", "counter"));
        slides.Add(new WrappedSlide("Current Level", $"Level {profile.Level}", "badge"));
        slides.Add(new WrappedSlide("Longest Streak", $"{profile.LongestStreak} days", "counter"));

        bool isPartialYear = year == today.Year;
        AnnualWrapped wrapped = AnnualWrapped.LoadHistorical(year, slides, isPartialYear, isPartialYear ? today : null);

        var slideModels = wrapped.Slides
            .Select(s => new WrappedSlideReadModel(s.Title, s.Metric, s.VisualizationType, s.IsShareable))
            .ToList();

        return new AnnualWrappedReadModel(wrapped.Year, wrapped.IsPartialYear, slideModels);
    }
}
