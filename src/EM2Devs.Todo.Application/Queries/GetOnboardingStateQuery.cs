using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Queries;

public sealed record GetOnboardingStateQuery : IRequest<Result<OnboardingStateReadModel>>;

public sealed class GetOnboardingStateQueryHandler
    : IRequestHandler<GetOnboardingStateQuery, Result<OnboardingStateReadModel>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IPlayerProfileRepository _profileRepository;

    public GetOnboardingStateQueryHandler(
        ITaskRepository taskRepository,
        IPlayerProfileRepository profileRepository)
    {
        _taskRepository = taskRepository;
        _profileRepository = profileRepository;
    }

    public async Task<Result<OnboardingStateReadModel>> Handle(GetOnboardingStateQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        IReadOnlyList<TodoTask> tasks = await _taskRepository.GetAllAsync(ct).ConfigureAwait(false);
        PlayerProfileReadModel profile = await _profileRepository.GetProfileAsync(ct).ConfigureAwait(false);

        int tasksCreated = tasks.Count;
        int tasksCompleted = tasks.Count(t => t.CompletedAt.HasValue);
        int questsCompleted = 0;

        IReadOnlyList<UnlockableFeature> unlocked = EngagementUnlockRegistry.EvaluateUnlocks(
            tasksCreated, tasksCompleted, profile.Level, questsCompleted);

        var allFeatures = Enum.GetValues<UnlockableFeature>();
        var previews = allFeatures
            .Where(f => !unlocked.Contains(f))
            .Select(f => new FeaturePreviewReadModel(
                f.ToString(),
                $"Unlock {f} by progressing further",
                $"Continue completing tasks and levelling up"))
            .ToList();

        return new OnboardingStateReadModel(
            unlocked.Select(f => f.ToString()).ToList(),
            profile.Level >= 3,
            previews);
    }
}
