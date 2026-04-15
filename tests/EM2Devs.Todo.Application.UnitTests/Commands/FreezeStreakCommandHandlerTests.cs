using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Application.Validators;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Exceptions;
using FluentValidation.Results;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Commands;

public sealed class FreezeStreakCommandHandlerTests
{
    private static readonly DateTimeOffset _fixedNow =
        new(2026, 4, 12, 10, 0, 0, TimeSpan.Zero);

    private sealed class FixedTimeProvider(DateTimeOffset fixedNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => fixedNow;
    }

    private readonly IPlayerProfileRepository _repository =
        Substitute.For<IPlayerProfileRepository>();
    private readonly TimeProvider _timeProvider = new FixedTimeProvider(_fixedNow);
    private readonly FreezeStreakCommandHandler _handler;

    public FreezeStreakCommandHandlerTests()
    {
        _handler = new FreezeStreakCommandHandler(_repository, _timeProvider);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_FreezeStreakAndReturnProfile_When_CommandIsValid()
    {
        // Given
        PlayerProfileReadModel returned = new(
            TotalXp: 0,
            Level: 1,
            XpToNextLevel: 50,
            CurrentStreak: 0,
            LongestStreak: 0,
            StreakFreeze: new StreakFreezeReadModel(
                FrozenAt: new DateOnly(2026, 4, 12),
                Days: 7,
                ExpiresAt: new DateOnly(2026, 4, 19)));
        _repository.GetProfileAsync(Arg.Any<CancellationToken>()).Returns(returned);

        // When
        Result<PlayerProfileReadModel> result =
            await _handler.Handle(new FreezeStreakCommand(7), CancellationToken.None);

        // Then
        result.IsSuccess.ShouldBeTrue();
        await _repository.Received(1).FreezeStreakAsync(
            new DateOnly(2026, 4, 12), 7, Arg.Any<CancellationToken>());
        PlayerProfileReadModel profile = result.Match(p => p, _ => null!);
        profile.StreakFreeze.ShouldNotBeNull();
        profile.StreakFreeze!.Days.ShouldBe(7);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnConflictError_When_AlreadyFrozen()
    {
        // Given — repo throws DomainException signalling already-frozen
        _repository
            .When(r => r.FreezeStreakAsync(
                Arg.Any<DateOnly>(), Arg.Any<int>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new DomainException("Streak is already frozen."));

        // When
        Result<PlayerProfileReadModel> result =
            await _handler.Handle(new FreezeStreakCommand(7), CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<ConflictError>();
    }

    [Theory]
    [Trait("Category", "Application")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(8)]
    [InlineData(100)]
    public void Validator_Should_Reject_OutOfRangeDays(int days)
    {
        FreezeStreakCommandValidator validator = new();
        ValidationResult result = validator.Validate(new FreezeStreakCommand(days));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(FreezeStreakCommand.Days));
    }

    [Theory]
    [Trait("Category", "Application")]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(7)]
    public void Validator_Should_Accept_InRangeDays(int days)
    {
        FreezeStreakCommandValidator validator = new();
        ValidationResult result = validator.Validate(new FreezeStreakCommand(days));
        result.IsValid.ShouldBeTrue();
    }
}
