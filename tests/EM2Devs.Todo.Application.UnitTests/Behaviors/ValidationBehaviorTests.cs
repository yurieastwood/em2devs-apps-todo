using EM2Devs.Todo.Application.Behaviors;
using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Validators;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using FluentValidation;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ShortCircuitWithValidationError_When_TitleIsEmpty()
    {
        // Given
        CreateTaskCommandValidator validator = new();
        ValidationBehavior<CreateTaskCommand, Result<TodoTask>> behavior = new([validator]);
        CreateTaskCommand command = new("");
        bool handlerCalled = false;

        // When
        Result<TodoTask> result = await behavior.Handle(
            command,
            () => { handlerCalled = true; return Task.FromResult(Result<TodoTask>.Success(null!)); },
            CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<ValidationError>();
        handlerCalled.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_CallHandler_When_ValidationPasses()
    {
        // Given
        CreateTaskCommandValidator validator = new();
        ValidationBehavior<CreateTaskCommand, Result<TodoTask>> behavior = new([validator]);
        CreateTaskCommand command = new("Valid title");
        bool handlerCalled = false;

        // When
        await behavior.Handle(
            command,
            () => { handlerCalled = true; return Task.FromResult(Result<TodoTask>.Success(null!)); },
            CancellationToken.None);

        // Then
        handlerCalled.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_IncludeFieldErrors_When_ValidationFails()
    {
        // Given
        CreateTaskCommandValidator validator = new();
        ValidationBehavior<CreateTaskCommand, Result<TodoTask>> behavior = new([validator]);
        CreateTaskCommand command = new("");

        // When
        Result<TodoTask> result = await behavior.Handle(
            command,
            () => Task.FromResult(Result<TodoTask>.Success(null!)),
            CancellationToken.None);

        // Then
        result.Match(_ => null!, e => e).ShouldBeOfType<ValidationError>()
            .Errors.ShouldNotBeNull();
    }
}
