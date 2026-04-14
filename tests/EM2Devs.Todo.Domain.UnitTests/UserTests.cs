using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Behaviour-driven tests for the <see cref="User"/> aggregate (Phase 0 multi-user auth).
/// Target: 100% Stryker mutation score.
/// </summary>
public sealed class UserTests
{
    private static readonly DateTimeOffset _createdAt = new(2026, 4, 12, 9, 30, 0, TimeSpan.Zero);
    private const string ValidEmail = "alice@waypoint.dev";
    private const string ValidHash = "bcrypt-hash-placeholder";
    private const string ValidDisplayName = "Alice";

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ConstructUser_When_AllInputsValid()
    {
        var user = User.Create(ValidEmail, ValidHash, ValidDisplayName, _createdAt);

        user.Id.ShouldNotBeNull();
        user.Id.Value.ShouldNotBe(Guid.Empty);
        user.Email.ShouldBe(ValidEmail);
        user.PasswordHash.ShouldBe(ValidHash);
        user.DisplayName.ShouldBe(ValidDisplayName);
        user.CreatedAt.ShouldBe(_createdAt);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseProvidedId_When_IdArgumentPassed()
    {
        var id = new UserId(new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

        var user = User.Create(ValidEmail, ValidHash, ValidDisplayName, _createdAt, id);

        user.Id.ShouldBe(id);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateDistinctIds_When_IdArgumentOmitted()
    {
        var u1 = User.Create(ValidEmail, ValidHash, ValidDisplayName, _createdAt);
        var u2 = User.Create(ValidEmail, ValidHash, ValidDisplayName, _createdAt);

        u1.Id.ShouldNotBe(u2.Id);
    }

    // -- Email validation --------------------------------------------------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_EmailIsNullOrWhitespace(string? email)
    {
        Action act = () => User.Create(email!, ValidHash, ValidDisplayName, _createdAt);

        var ex = act.ShouldThrow<DomainException>();
        ex.Message.ShouldBe("Email cannot be empty.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_EmailMissingAtSign()
    {
        Action act = () => User.Create("not-an-email", ValidHash, ValidDisplayName, _createdAt);

        var ex = act.ShouldThrow<DomainException>();
        ex.Message.ShouldBe("Email must contain '@'.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_EmailExceedsMaxLength()
    {
        // 255 chars total: 243 a's + "@example.com" (12) = 255
        string tooLong = new string('a', 243) + "@example.com";
        tooLong.Length.ShouldBe(255);

        Action act = () => User.Create(tooLong, ValidHash, ValidDisplayName, _createdAt);

        var ex = act.ShouldThrow<DomainException>();
        ex.Message.ShouldBe("Email cannot exceed 254 characters.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Accept_When_EmailExactlyAtMaxLength()
    {
        // 254 chars total: 242 a's + "@example.com" (12) = 254
        string atLimit = new string('a', 242) + "@example.com";
        atLimit.Length.ShouldBe(254);

        var user = User.Create(atLimit, ValidHash, ValidDisplayName, _createdAt);

        user.Email.ShouldBe(atLimit);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_LengthBeforeAtSignValidation_When_TooLongButMissingAt()
    {
        // Ensures the length check precedes the '@' check — a too-long input
        // without an '@' still fails with the length message.
        string tooLongNoAt = new string('a', 255);

        Action act = () => User.Create(tooLongNoAt, ValidHash, ValidDisplayName, _createdAt);

        var ex = act.ShouldThrow<DomainException>();
        ex.Message.ShouldBe("Email cannot exceed 254 characters.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExposeMaxEmailLengthConstant()
    {
        User.MaxEmailLength.ShouldBe(254);
    }

    // -- Password hash validation -----------------------------------------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\n")]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_PasswordHashIsNullOrWhitespace(string? hash)
    {
        Action act = () => User.Create(ValidEmail, hash!, ValidDisplayName, _createdAt);

        var ex = act.ShouldThrow<DomainException>();
        ex.Message.ShouldBe("Password hash cannot be empty.");
    }

    // -- Display name validation ------------------------------------------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("     ")]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_DisplayNameIsNullOrWhitespace(string? name)
    {
        Action act = () => User.Create(ValidEmail, ValidHash, name!, _createdAt);

        var ex = act.ShouldThrow<DomainException>();
        ex.Message.ShouldBe("Display name cannot be empty.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_DisplayNameExceedsMaxLength()
    {
        string tooLong = new string('x', 101);

        Action act = () => User.Create(ValidEmail, ValidHash, tooLong, _createdAt);

        var ex = act.ShouldThrow<DomainException>();
        ex.Message.ShouldBe("Display name cannot exceed 100 characters.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Accept_When_DisplayNameExactlyAtMaxLength()
    {
        string atLimit = new string('x', 100);

        var user = User.Create(ValidEmail, ValidHash, atLimit, _createdAt);

        user.DisplayName.ShouldBe(atLimit);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Accept_When_DisplayNameExactlyOneCharacter()
    {
        var user = User.Create(ValidEmail, ValidHash, "a", _createdAt);

        user.DisplayName.ShouldBe("a");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExposeDisplayNameLengthConstants()
    {
        User.MinDisplayNameLength.ShouldBe(1);
        User.MaxDisplayNameLength.ShouldBe(100);
    }

    // -- ChangePassword ---------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UpdatePasswordHash_When_ChangePasswordCalled()
    {
        var user = User.Create(ValidEmail, ValidHash, ValidDisplayName, _createdAt);

        user.ChangePassword("new-hash");

        user.PasswordHash.ShouldBe("new-hash");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_ChangePasswordReceivesNullOrWhitespace(string? hash)
    {
        var user = User.Create(ValidEmail, ValidHash, ValidDisplayName, _createdAt);

        Action act = () => user.ChangePassword(hash!);

        var ex = act.ShouldThrow<DomainException>();
        ex.Message.ShouldBe("Password hash cannot be empty.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_LeavePasswordHashUnchanged_When_ChangePasswordRejected()
    {
        var user = User.Create(ValidEmail, ValidHash, ValidDisplayName, _createdAt);

        try
        {
            user.ChangePassword("");
        }
        catch (DomainException)
        {
            // expected
        }

        user.PasswordHash.ShouldBe(ValidHash);
    }

    // -- UpdateDisplayName ------------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UpdateDisplayName_When_ValidNameProvided()
    {
        var user = User.Create(ValidEmail, ValidHash, ValidDisplayName, _createdAt);

        user.UpdateDisplayName("Bob");

        user.DisplayName.ShouldBe("Bob");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_UpdateDisplayNameReceivesNullOrWhitespace(string? name)
    {
        var user = User.Create(ValidEmail, ValidHash, ValidDisplayName, _createdAt);

        Action act = () => user.UpdateDisplayName(name!);

        var ex = act.ShouldThrow<DomainException>();
        ex.Message.ShouldBe("Display name cannot be empty.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_UpdateDisplayNameExceedsMaxLength()
    {
        var user = User.Create(ValidEmail, ValidHash, ValidDisplayName, _createdAt);
        string tooLong = new string('x', 101);

        Action act = () => user.UpdateDisplayName(tooLong);

        var ex = act.ShouldThrow<DomainException>();
        ex.Message.ShouldBe("Display name cannot exceed 100 characters.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowUpdateDisplayName_ExactlyAtMaxLength()
    {
        var user = User.Create(ValidEmail, ValidHash, ValidDisplayName, _createdAt);
        string atLimit = new string('x', 100);

        user.UpdateDisplayName(atLimit);

        user.DisplayName.ShouldBe(atLimit);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_LeaveDisplayNameUnchanged_When_UpdateRejected()
    {
        var user = User.Create(ValidEmail, ValidHash, ValidDisplayName, _createdAt);

        try
        {
            user.UpdateDisplayName("");
        }
        catch (DomainException)
        {
            // expected
        }

        user.DisplayName.ShouldBe(ValidDisplayName);
    }

    // -- UserId value object ----------------------------------------------

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateDistinctUserIds_When_UserIdNewCalled()
    {
        var a = UserId.New();
        var b = UserId.New();

        a.ShouldNotBe(b);
        a.Value.ShouldNotBe(Guid.Empty);
        b.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CompareEqual_When_UserIdsHaveSameGuid()
    {
        var guid = Guid.NewGuid();

        new UserId(guid).ShouldBe(new UserId(guid));
    }
}
