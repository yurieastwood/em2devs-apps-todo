using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for CheckInMessage value object.
/// Maps to: docs/features/social/accountability-partners.feature
/// Rule: "Partners see daily summaries, not task-level detail"
/// Scenario: "Send a check-in message to partner"
/// Scenario: "Partner check-in messages are limited scope"
/// </summary>
public sealed class CheckInMessageTests
{
    private static readonly Guid _senderId = Guid.NewGuid();
    private static readonly DateTimeOffset _now = new(2026, 3, 15, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateMessage_When_ValidInputProvided()
    {
        // Given / When
        var message = new CheckInMessage("Great job today!", _now, _senderId);

        // Then
        message.Text.ShouldBe("Great job today!");
        message.SentAt.ShouldBe(_now);
        message.SenderId.ShouldBe(_senderId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TextIsEmpty()
    {
        var ex = Should.Throw<DomainException>(
            () => new CheckInMessage("", _now, _senderId));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TextIsWhitespace()
    {
        var ex = Should.Throw<DomainException>(
            () => new CheckInMessage("   ", _now, _senderId));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TextExceedsMaxLength()
    {
        string longText = new('x', CheckInMessage.MaxLength + 1);
        var ex = Should.Throw<DomainException>(
            () => new CheckInMessage(longText, _now, _senderId));
        ex.Message.ShouldContain($"{CheckInMessage.MaxLength}");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowMessage_When_TextIsExactlyMaxLength()
    {
        string text = new('x', CheckInMessage.MaxLength);
        var message = new CheckInMessage(text, _now, _senderId);
        message.Text.Length.ShouldBe(CheckInMessage.MaxLength);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SenderIdIsEmpty()
    {
        var ex = Should.Throw<DomainException>(
            () => new CheckInMessage("Hello!", _now, Guid.Empty));
        ex.Message.ShouldContain("Sender ID cannot be empty");
    }
}
