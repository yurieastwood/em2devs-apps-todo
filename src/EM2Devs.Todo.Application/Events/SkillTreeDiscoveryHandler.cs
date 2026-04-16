using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Events;

public sealed class SkillTreeDiscoveryHandler : INotificationHandler<TaskCompletedEvent>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IPlayerProfileRepository _profileRepository;

    public SkillTreeDiscoveryHandler(
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

        var tagCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (TodoTask task in tasks)
        {
            if (!task.CompletedAt.HasValue)
            {
                continue;
            }

            foreach (Tag tag in task.Tags)
            {
                tagCounts.TryGetValue(tag.Value, out int count);
                tagCounts[tag.Value] = count + 1;
            }
        }

        foreach ((string category, int count) in tagCounts)
        {
            if (SkillTreeDiscovery.TryGetTreeType(category, out SkillTreeType treeType)
                && count >= SkillTreeDiscovery.DiscoveryThreshold(treeType))
            {
                await _profileRepository.DiscoverSkillTreeAsync(treeType, ct).ConfigureAwait(false);
            }
        }
    }
}
