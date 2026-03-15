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

    // TODO: Human defines these scenarios, agent implements the production code:
    //
    // Should_TransitionToInProgress_When_TaskIsTodo
    // Should_TransitionToDone_When_TaskIsInProgress
    // Should_ThrowDomainException_When_TransitioningFromTodoToDone
    // Should_ThrowDomainException_When_TransitioningFromDoneToAnyStatus
}
