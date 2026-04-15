using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Application.Events;

/// <summary>
/// Converts achievement-style application events into persisted in-app notifications
/// for the current user. Listens to <see cref="LevelUpEvent"/>,
/// <see cref="StreakMilestoneReachedEvent"/>, and <see cref="QuestCompletedEvent"/>;
/// each produces one <see cref="NotificationType.AchievementAlert"/> row that the
/// inbox endpoint returns.
/// </summary>
public sealed class NotificationCreationHandler :
    INotificationHandler<LevelUpEvent>,
    INotificationHandler<StreakMilestoneReachedEvent>,
    INotificationHandler<QuestCompletedEvent>
{
    private const int AchievementAutoDismissSeconds = 5;

    private readonly ICurrentUser _currentUser;
    private readonly INotificationRepository _repository;

    public NotificationCreationHandler(ICurrentUser currentUser, INotificationRepository repository)
    {
        _currentUser = currentUser;
        _repository = repository;
    }

    public Task Handle(LevelUpEvent notification, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(notification);
        string message = $"Level up! You reached level {notification.NewLevel}.";
        return CreateAsync(message, ct);
    }

    public Task Handle(StreakMilestoneReachedEvent notification, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(notification);
        string message = $"Streak milestone: {notification.Label} ({notification.StreakDays} days)!";
        return CreateAsync(message, ct);
    }

    public Task Handle(QuestCompletedEvent notification, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(notification);
        string message = $"Quest completed: {notification.Title.Value}!";
        return CreateAsync(message, ct);
    }

    private Task CreateAsync(string message, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
        {
            return Task.CompletedTask;
        }

        Notification entity = Notification.CreateForUser(
            _currentUser.UserId,
            NotificationType.AchievementAlert,
            message,
            AchievementAutoDismissSeconds);
        return _repository.AddAsync(entity, ct);
    }
}
