using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Application.Validators;
using EM2Devs.Todo.Domain;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Commands;

[Trait("Category", "Application")]
public sealed class SaveWeeklyReviewCommandHandlerTests
{
    private static readonly DateTimeOffset _fixedNow = new(2026, 4, 15, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly _weekOf = new(2026, 4, 12);

    private readonly IWeeklyReflectionRepository _repo = Substitute.For<IWeeklyReflectionRepository>();
    private readonly TimeProvider _timeProvider = new FixedTimeProvider(_fixedNow);
    private readonly SaveWeeklyReviewCommandHandler _handler;

    public SaveWeeklyReviewCommandHandlerTests()
    {
        _handler = new SaveWeeklyReviewCommandHandler(_repo, _timeProvider);
    }

    private sealed class FixedTimeProvider(DateTimeOffset fixedNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => fixedNow;
    }

    [Fact]
    public async Task Should_PersistReflection_With_DerivedWeekOfSunday()
    {
        SaveWeeklyReviewCommand cmd = new("Shipped slice", "Context switches", "Deep-work block");

        Result<WeeklyReflectionReadModel> result = await _handler.Handle(cmd, default);

        result.IsSuccess.ShouldBeTrue();
        WeeklyReflectionReadModel saved = result.Match(r => r, _ => throw new Xunit.Sdk.XunitException("expected success"));
        saved.WhatWentWell.ShouldBe("Shipped slice");
        saved.WhatDragged.ShouldBe("Context switches");
        saved.Adjustment.ShouldBe("Deep-work block");
        saved.SavedAt.ShouldBe(_fixedNow);
        await _repo.Received(1).SaveAsync(
            _weekOf, "Shipped slice", "Context switches", "Deep-work block",
            _fixedNow, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_UseExplicitWeekOf_When_Supplied()
    {
        DateOnly explicitWeek = new(2026, 3, 29);
        SaveWeeklyReviewCommand cmd = new("a", "b", "c", explicitWeek);

        Result<WeeklyReflectionReadModel> result = await _handler.Handle(cmd, default);

        result.IsSuccess.ShouldBeTrue();
        await _repo.Received(1).SaveAsync(
            explicitWeek, "a", "b", "c", _fixedNow, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_TrimWhitespace_FromReflectionFields()
    {
        SaveWeeklyReviewCommand cmd = new("  well  ", "\tdragged\n", " adjust ");

        Result<WeeklyReflectionReadModel> result = await _handler.Handle(cmd, default);

        WeeklyReflectionReadModel saved = result.Match(r => r, _ => throw new Xunit.Sdk.XunitException("expected success"));
        saved.WhatWentWell.ShouldBe("well");
        saved.WhatDragged.ShouldBe("dragged");
        saved.Adjustment.ShouldBe("adjust");
    }

    [Fact]
    public async Task Should_ThrowArgumentNullException_When_RequestIsNull()
    {
        await Should.ThrowAsync<ArgumentNullException>(async () => await _handler.Handle(null!, default));
    }

    [Theory]
    [InlineData("", "dragged", "adjust")]
    [InlineData("well", "", "adjust")]
    [InlineData("well", "dragged", "")]
    public void Validator_Should_RejectEmptyFields(string wentWell, string dragged, string adjust)
    {
        SaveWeeklyReviewCommandValidator validator = new();
        ValidationResult r = validator.Validate(new SaveWeeklyReviewCommand(wentWell, dragged, adjust));
        r.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Validator_Should_AcceptValidCommand()
    {
        SaveWeeklyReviewCommandValidator validator = new();
        ValidationResult r = validator.Validate(new SaveWeeklyReviewCommand("a", "b", "c"));
        r.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validator_Should_RejectOverlongField()
    {
        SaveWeeklyReviewCommandValidator validator = new();
        string tooLong = new('x', SaveWeeklyReviewCommandValidator.MaxReflectionLength + 1);
        ValidationResult r = validator.Validate(new SaveWeeklyReviewCommand(tooLong, "b", "c"));
        r.IsValid.ShouldBeFalse();
    }
}
