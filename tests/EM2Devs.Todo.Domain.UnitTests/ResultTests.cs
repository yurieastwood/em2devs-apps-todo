using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Gate 4: Scenario-driven tests for Result&lt;T&gt; type.
/// Tests encode the discriminated union behavior (ADR-018).
/// </summary>
public sealed class ResultTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeSuccess_When_CreatedWithValue()
    {
        // When
        Result<int> result = Result<int>.Success(42);

        // Then
        result.IsSuccess.ShouldBeTrue();
        result.IsError.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeError_When_CreatedWithError()
    {
        // When
        Result<int> result = Result<int>.Failure(new NotFoundError("Not found"));

        // Then
        result.IsError.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnValue_When_MatchingSuccess()
    {
        // Given
        Result<string> result = Result<string>.Success("hello");

        // When
        string output = result.Match(
            value => value.ToUpperInvariant(),
            error => error.Message);

        // Then
        output.ShouldBe("HELLO");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnErrorMessage_When_MatchingError()
    {
        // Given
        Result<string> result = Result<string>.Failure(new ValidationError("Title is required"));

        // When
        string output = result.Match(
            value => value,
            error => error.Message);

        // Then
        output.ShouldBe("Title is required");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ContainFieldErrors_When_ValidationErrorHasErrors()
    {
        // Given
        Dictionary<string, string[]> errors = new()
        {
            ["Title"] = ["Title is required", "Title must not exceed 200 characters"]
        };

        // When
        ValidationError error = new("Validation failed", errors);

        // Then
        error.Errors.ShouldNotBeNull();
        error.Errors!["Title"].Length.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ImplicitlyConvertValue_When_AssignedToResult()
    {
        // When
        Result<int> result = 42;

        // Then
        result.IsSuccess.ShouldBeTrue();
        result.Match(v => v, _ => -1).ShouldBe(42);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ImplicitlyConvertError_When_AssignedToResult()
    {
        // When
        Result<int> result = new ConflictError("Already exists");

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => "", e => e.Message).ShouldBe("Already exists");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DistinguishErrorTypes_When_Matching()
    {
        // Given
        Result<string> notFound = new NotFoundError("Task not found");
        Result<string> validation = new ValidationError("Bad input");
        Result<string> conflict = new ConflictError("Already done");

        // When/Then
        notFound.Match(_ => "", e => e is NotFoundError ? "404" : "other").ShouldBe("404");
        validation.Match(_ => "", e => e is ValidationError ? "400" : "other").ShouldBe("400");
        conflict.Match(_ => "", e => e is ConflictError ? "409" : "other").ShouldBe("409");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_OnSuccessIsNull()
    {
        // Given
        Result<int> result = 42;

        // When/Then
        Should.Throw<ArgumentNullException>(() => result.Match(null!, _ => ""));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_OnErrorIsNull()
    {
        // Given
        Result<int> result = new NotFoundError("missing");

        // When/Then
        Should.Throw<ArgumentNullException>(() => result.Match(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture), (Func<ResultError, string>)null!));
    }
}
