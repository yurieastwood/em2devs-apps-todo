using EM2Devs.Todo.Application.Events;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Events;

/// <summary>
/// Tests for QuestCompletionXpHandler.
/// Scenario: "Complete the final task in a quest" — quest completion bonus XP should be awarded.
/// </summary>
public sealed class QuestCompletionXpHandlerTests
{
    private readonly IPlayerProfileRepository _profileRepo = Substitute.For<IPlayerProfileRepository>();
    private readonly QuestCompletionXpHandler _handler;

    public QuestCompletionXpHandlerTests()
    {
        _handler = new QuestCompletionXpHandler(_profileRepo);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_AwardBonusXp_When_QuestCompleted()
    {
        // Given
        QuestCompletedEvent evt = new(QuestId.New(), new QuestTitle("Prepare presentation"));

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then — bonus XP awarded
        await _profileRepo.Received(1).AwardXpAsync(
            QuestCompletionXpHandler.QuestCompletionBonusXp,
            null,
            Arg.Any<DateOnly?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public void Should_HaveCorrectBonusXpValue()
    {
        // Then — quest completion bonus is 50 XP
        QuestCompletionXpHandler.QuestCompletionBonusXp.Value.ShouldBe(50);
    }
}
