using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

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
    private readonly INotificationPublisher _publisher;

    public NotificationCreationHandler(
        ICurrentUser currentUser,
        INotificationRepository repository,
        INotificationPublisher publisher)
    {
        _currentUser = currentUser;
        _repository = repository;
        _publisher = publisher;
    }

    public async Task Handle(LevelUpEvent notification, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(notification);
        string message = $"Level up! You reached level {notification.NewLevel}.";
        await CreateAsync(message, ct).ConfigureAwait(false);

        IReadOnlyList<UnlockableFeature> newFeatures =
            FeatureUnlockRegistry.GetNewlyUnlockedFeatures(notification.NewLevel);
        if (newFeatures.Count > 0)
        {
            string featureNames = string.Join(", ", newFeatures.Select(f => f.ToString()));
            await CreateAsync($"New features unlocked: {featureNames}!", ct).ConfigureAwait(false);
        }

        LevelMilestone? milestone = LevelMilestone.ForLevel(notification.NewLevel);
        if (milestone is not null)
        {
            await CreateAsync(
                $"Level milestone reached: {milestone.Label}! You hit level {milestone.Level}.",
                ct).ConfigureAwait(false);
        }
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

    private async Task CreateAsync(string message, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
        {
            return;
        }

        Notification entity = Notification.CreateForUser(
            _currentUser.UserId,
            NotificationType.AchievementAlert,
            message,
            AchievementAutoDismissSeconds);
        await _repository.AddAsync(entity, ct).ConfigureAwait(false);
        await _publisher.PublishAsync(_currentUser.UserId, entity, ct).ConfigureAwait(false);
    }
}
