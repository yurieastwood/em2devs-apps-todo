using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Application.Events;

public sealed class TimelineRecordingHandler :
    INotificationHandler<LevelUpEvent>,
    INotificationHandler<StreakMilestoneReachedEvent>,
    INotificationHandler<QuestCompletedEvent>
{
    private readonly ITimelineRepository _repository;
    private readonly ICurrentUser _currentUser;

    public TimelineRecordingHandler(ITimelineRepository repository, ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task Handle(LevelUpEvent notification, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (!_currentUser.IsAuthenticated)
        {
            return;
        }

        var evt = TimelineEvent.Create(
            TimelineEventType.LevelUp,
            DateTimeOffset.UtcNow,
            $"Reached level {notification.NewLevel}");
        await _repository.AddAsync(evt, ct).ConfigureAwait(false);
    }

    public async Task Handle(StreakMilestoneReachedEvent notification, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (!_currentUser.IsAuthenticated)
        {
            return;
        }

        var evt = TimelineEvent.Create(
            TimelineEventType.StreakMilestone,
            DateTimeOffset.UtcNow,
            $"{notification.Label} — {notification.StreakDays}-day streak");
        await _repository.AddAsync(evt, ct).ConfigureAwait(false);
    }

    public async Task Handle(QuestCompletedEvent notification, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (!_currentUser.IsAuthenticated)
        {
            return;
        }

        var evt = TimelineEvent.Create(
            TimelineEventType.QuestCompleted,
            DateTimeOffset.UtcNow,
            $"Quest completed: {notification.Title.Value}");
        await _repository.AddAsync(evt, ct).ConfigureAwait(false);
    }
}
