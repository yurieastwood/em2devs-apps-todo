using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Gate 4: Scenario-driven tests for TodoTask entity.
/// Tests encode behaviors, not methods (ADR-0003).
/// </summary>
public sealed class TodoTaskTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateWithTodoStatus_When_NewTaskIsCreated()
    {
        // Given
        var title = new TaskTitle("Write architecture tests");

        // When
        var task = TodoTask.Create(title);

        // Then
        task.Status.ShouldBe(TaskStatus.Todo);
        task.Title.ShouldBe(title);
        task.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TitleIsEmpty()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(() => new TaskTitle(""));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TitleExceedsMaxLength()
    {
        // Given
        string longTitle = new string('x', 201);

        // When / Then
        var ex = Should.Throw<DomainException>(() => new TaskTitle(longTitle));
        ex.Message.ShouldContain("cannot exceed 200");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptTitle_When_TitleIsExactlyMaxLength()
    {
        // Given
        string maxTitle = new string('x', 200);

        // When
        var title = new TaskTitle(maxTitle);

        // Then
        title.Value.ShouldBe(maxTitle);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TitleIsNull()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(() => new TaskTitle(null!));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TitleIsWhitespaceOnly()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(() => new TaskTitle("   "));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TitleIsControlCharactersOnly()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(() => new TaskTitle("\x01\x02\x03"));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptTitle_When_TitleContainsMixedValidAndWhitespace()
    {
        // Given / When
        var title = new TaskTitle("  valid title  ");

        // Then
        title.Value.ShouldBe("  valid title  ");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptTitle_When_CombiningCharactersMakeVisualLength200()
    {
        // Given — 200 base chars each followed by a combining acute accent (U+0301)
        // string.Length = 400 code units, EnumerateRunes().Count() = 400 runes,
        // but StringInfo.LengthInTextElements = 200 grapheme clusters
        string title200Graphemes = string.Concat(Enumerable.Range(0, 200).Select(_ => "a\u0301"));

        // When
        var title = new TaskTitle(title200Graphemes);

        // Then
        title.Value.ShouldBe(title200Graphemes);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_GraphemeLengthExceeds200()
    {
        // Given — 201 grapheme clusters using combining characters
        string title201Graphemes = string.Concat(Enumerable.Range(0, 201).Select(_ => "e\u0300"));

        // When / Then
        var ex = Should.Throw<DomainException>(() => new TaskTitle(title201Graphemes));
        ex.Message.ShouldContain("cannot exceed 200");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TransitionToInProgress_When_TaskIsTodo()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Start working"));

        // When
        task.MoveToInProgress();

        // Then
        task.Status.ShouldBe(TaskStatus.InProgress);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TransitionToDone_When_TaskIsInProgress()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Finish working"));
        task.MoveToInProgress();

        // When
        task.MarkAsDone();

        // Then
        task.Status.ShouldBe(TaskStatus.Done);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TransitioningFromTodoToDone()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Skip ahead"));

        // When / Then
        var ex = Should.Throw<DomainException>(() => task.MarkAsDone());
        ex.Message.ShouldContain("Cannot transition");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TransitioningFromDoneToAnyStatus()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Already finished"));
        task.MoveToInProgress();
        task.MarkAsDone();

        // When / Then
        var exToInProgress = Should.Throw<DomainException>(() => task.MoveToInProgress());
        exToInProgress.Message.ShouldContain("Cannot transition");

        var exToDone = Should.Throw<DomainException>(() => task.MarkAsDone());
        exToDone.Message.ShouldContain("Cannot transition");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeBossTask_When_NewTaskIsCreated()
    {
        // Given / When
        var task = TodoTask.Create(new TaskTitle("Normal task"));

        // Then
        task.IsBossTask.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BecomeBossTask_When_Promoted()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Hard task"));

        // When
        task.PromoteToBossTask();

        // Then
        task.IsBossTask.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StopBeingBossTask_When_Demoted()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Was hard"));
        task.PromoteToBossTask();

        // When
        task.DemoteFromBossTask();

        // Then
        task.IsBossTask.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RemainBossTask_When_PromotedTwice()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Already boss"));
        task.PromoteToBossTask();

        // When
        task.PromoteToBossTask();

        // Then
        task.IsBossTask.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_PromotingDoneTask()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Finished"));
        task.MoveToInProgress();
        task.MarkAsDone();

        // When / Then
        var ex = Should.Throw<DomainException>(() => task.PromoteToBossTask());
        ex.Message.ShouldContain("Cannot promote");
    }
}
