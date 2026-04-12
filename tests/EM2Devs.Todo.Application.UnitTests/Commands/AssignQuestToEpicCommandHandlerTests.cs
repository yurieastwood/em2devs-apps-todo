using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Commands;

/// <summary>
/// Tests for AssignQuestToEpicCommandHandler.
/// Scenario: "A quest cannot belong to more than one epic"
/// </summary>
public sealed class AssignQuestToEpicCommandHandlerTests
{
    private readonly IEpicRepository _epicRepo = Substitute.For<IEpicRepository>();
    private readonly IQuestRepository _questRepo = Substitute.For<IQuestRepository>();
    private readonly AssignQuestToEpicCommandHandler _handler;

    public AssignQuestToEpicCommandHandlerTests()
    {
        _handler = new AssignQuestToEpicCommandHandler(_epicRepo, _questRepo);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_AssignQuestToEpic_When_QuestHasNoEpic()
    {
        // Given
        Epic epic = Epic.Create(new EpicTitle("Launch MVP"), "Ship first version");
        Quest quest = Quest.Create(new QuestTitle("Build authentication"), "Auth module");

        _epicRepo.GetByIdAsync(epic.Id, Arg.Any<CancellationToken>())
            .Returns(epic);
        _questRepo.GetByIdAsync(quest.Id, Arg.Any<CancellationToken>())
            .Returns(quest);

        AssignQuestToEpicCommand command = new(epic.Id.Value, quest.Id.Value);

        // When
        Result<Epic> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsSuccess.ShouldBeTrue();
        epic.Quests.Count.ShouldBe(1);
        quest.EpicId.ShouldBe(epic.Id);
        await _questRepo.Received(1).SaveAsync(quest, Arg.Any<CancellationToken>());
        await _epicRepo.Received(1).SaveAsync(epic, Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnConflictError_When_QuestAlreadyBelongsToAnEpic()
    {
        // Given — quest already assigned to another epic
        Epic existingEpic = Epic.Create(new EpicTitle("Launch MVP"), "Ship first version");
        Epic newEpic = Epic.Create(new EpicTitle("Side Project"), "Fun stuff");
        Quest quest = Quest.Create(new QuestTitle("Build authentication"), "Auth module");
        quest.AssignToEpic(existingEpic.Id);

        _epicRepo.GetByIdAsync(newEpic.Id, Arg.Any<CancellationToken>())
            .Returns(newEpic);
        _questRepo.GetByIdAsync(quest.Id, Arg.Any<CancellationToken>())
            .Returns(quest);

        AssignQuestToEpicCommand command = new(newEpic.Id.Value, quest.Id.Value);

        // When
        Result<Epic> result = await _handler.Handle(command, CancellationToken.None);

        // Then — conflict error indicating quest already belongs to an epic
        result.IsError.ShouldBeTrue();
        ResultError error = result.Match(_ => null!, e => e);
        error.ShouldBeOfType<ConflictError>();
        error.Message.ShouldContain("already belongs to an epic");
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnNotFoundError_When_EpicDoesNotExist()
    {
        // Given
        _epicRepo.GetByIdAsync(Arg.Any<EpicId>(), Arg.Any<CancellationToken>())
            .Returns((Epic?)null);

        AssignQuestToEpicCommand command = new(Guid.NewGuid(), Guid.NewGuid());

        // When
        Result<Epic> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<NotFoundError>();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnNotFoundError_When_QuestDoesNotExist()
    {
        // Given
        Epic epic = Epic.Create(new EpicTitle("Launch MVP"), "Ship first version");
        _epicRepo.GetByIdAsync(epic.Id, Arg.Any<CancellationToken>())
            .Returns(epic);
        _questRepo.GetByIdAsync(Arg.Any<QuestId>(), Arg.Any<CancellationToken>())
            .Returns((Quest?)null);

        AssignQuestToEpicCommand command = new(epic.Id.Value, Guid.NewGuid());

        // When
        Result<Epic> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<NotFoundError>();
    }
}
