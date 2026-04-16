using EM2Devs.Todo.Application.Events;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Events;

[Trait("Category", "Application")]
public sealed class SkillTreeDiscoveryHandlerTests
{
    private static readonly Guid _userId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private readonly ITaskRepository _taskRepository = Substitute.For<ITaskRepository>();
    private readonly IPlayerProfileRepository _profileRepository = Substitute.For<IPlayerProfileRepository>();
    private readonly SkillTreeDiscoveryHandler _handler;

    public SkillTreeDiscoveryHandlerTests()
    {
        _handler = new SkillTreeDiscoveryHandler(_taskRepository, _profileRepository);
    }

    [Fact]
    public async Task Should_DiscoverBuilderTree_When_SideProjectTagReachesThreshold()
    {
        var tasks = new List<TodoTask>();
        for (int i = 0; i < 10; i++)
        {
            TodoTask task = TodoTask.Create(_userId, new TaskTitle($"Side project {i}"));
            task.AddTag(Tag.From("side-project"));
            task.MoveToInProgress();
            task.MarkAsDone();
            tasks.Add(task);
        }

        _taskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(tasks);

        await _handler.Handle(
            new TaskCompletedEvent(TaskId.New(), new TaskTitle("trigger")),
            CancellationToken.None);

        await _profileRepository.Received(1).DiscoverSkillTreeAsync(
            SkillTreeType.Builder, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_NotDiscover_When_BelowThreshold()
    {
        var tasks = new List<TodoTask>();
        for (int i = 0; i < 5; i++)
        {
            TodoTask task = TodoTask.Create(_userId, new TaskTitle($"Side project {i}"));
            task.AddTag(Tag.From("side-project"));
            task.MoveToInProgress();
            task.MarkAsDone();
            tasks.Add(task);
        }

        _taskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(tasks);

        await _handler.Handle(
            new TaskCompletedEvent(TaskId.New(), new TaskTitle("trigger")),
            CancellationToken.None);

        await _profileRepository.DidNotReceive().DiscoverSkillTreeAsync(
            Arg.Any<SkillTreeType>(), Arg.Any<CancellationToken>());
    }
}
