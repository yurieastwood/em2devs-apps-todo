using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Services;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Events;

public sealed class TitleEvaluationHandler : INotificationHandler<TaskCompletedEvent>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IPlayerProfileRepository _profileRepository;

    public TitleEvaluationHandler(
        ITaskRepository taskRepository,
        IPlayerProfileRepository profileRepository)
    {
        _taskRepository = taskRepository;
        _profileRepository = profileRepository;
    }

    public async Task Handle(TaskCompletedEvent notification, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(notification);

        IReadOnlyList<TodoTask> tasks = await _taskRepository.GetAllAsync(ct).ConfigureAwait(false);
        PlayerProfileReadModel profile = await _profileRepository.GetProfileAsync(ct).ConfigureAwait(false);

        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        var completedDates = tasks
            .Where(t => t.CompletedAt.HasValue)
            .Select(t => new TitleQualifyingAction(DateOnly.FromDateTime(t.CompletedAt!.Value.UtcDateTime)))
            .ToList();

        int bossTaskCount = tasks.Count(t => t.IsBossTask && t.CompletedAt.HasValue);

        foreach (TitleType titleType in Enum.GetValues<TitleType>())
        {
            TitleRequirement requirement = TitleRequirement.For(titleType);
            IReadOnlyCollection<TitleQualifyingAction> actions = titleType switch
            {
                TitleType.BossSlayer => Enumerable.Range(0, bossTaskCount)
                    .Select(_ => new TitleQualifyingAction(today)).ToList(),
                TitleType.StreakMaster => Enumerable.Range(0, profile.CurrentStreak)
                    .Select(i => new TitleQualifyingAction(today.AddDays(-i))).ToList(),
                _ => completedDates
            };

            TitleProgress progress = TitleEvaluator.Evaluate(requirement, actions, today);

            if (progress.IsEarned)
            {
                Title title = new(titleType, today);
                await _profileRepository.AwardTitleAsync(title, ct).ConfigureAwait(false);
            }
        }
    }
}
