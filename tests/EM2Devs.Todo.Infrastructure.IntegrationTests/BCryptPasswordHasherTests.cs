using EM2Devs.Todo.Infrastructure.Auth;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Infrastructure.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new();

    [Fact]
    public void Hash_Then_Verify_ShouldRoundTrip()
    {
        string hash = _hasher.Hash("demo1234");

        hash.ShouldNotBeNullOrWhiteSpace();
        hash.ShouldNotBe("demo1234");
        _hasher.Verify("demo1234", hash).ShouldBeTrue();
    }

    [Fact]
    public void Verify_ShouldReturnFalse_ForWrongPassword()
    {
        string hash = _hasher.Hash("demo1234");

        _hasher.Verify("wrong-password", hash).ShouldBeFalse();
    }

    [Fact]
    public void Hash_ShouldProduceDistinctHashes_ForSamePlaintext()
    {
        // BCrypt salts randomly; two hashes of the same plaintext must differ.
        string a = _hasher.Hash("demo1234");
        string b = _hasher.Hash("demo1234");

        a.ShouldNotBe(b);
        _hasher.Verify("demo1234", a).ShouldBeTrue();
        _hasher.Verify("demo1234", b).ShouldBeTrue();
    }
}
