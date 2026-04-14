using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

public sealed class TagTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NormaliseToLowercase_When_Created()
    {
        Tag.From("Work").Value.ShouldBe("work");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TrimWhitespace_When_Created()
    {
        Tag.From("  travel  ").Value.ShouldBe("travel");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TreatEquivalentTagsAsEqual_When_Compared()
    {
        Tag.From("Work").ShouldBe(Tag.From("work"));
        Tag.From("Work").ShouldBe(Tag.From(" WORK "));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EmptyOrWhitespace()
    {
        Should.Throw<DomainException>(() => Tag.From(""));
        Should.Throw<DomainException>(() => Tag.From("   "));
        Should.Throw<DomainException>(() => Tag.From(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ExceedsMaxLength()
    {
        string tooLong = new('x', Tag.MaxLength + 1);
        Should.Throw<DomainException>(() => Tag.From(tooLong))
            .Message.ShouldContain("50");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptTag_When_ExactlyMaxLength()
    {
        string boundary = new('x', Tag.MaxLength);
        Tag.From(boundary).Value.Length.ShouldBe(Tag.MaxLength);
    }
}
