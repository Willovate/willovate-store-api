#pragma warning disable CA1707

using Willovate.Store.Api.Services;

namespace Willovate.Store.Api.Tests;

public sealed class PasswordServiceTests
{
    private readonly PasswordService passwordService = new();

    [Fact]
    public void HashPassword_ReturnsHashDifferentFromPlaintext()
    {
        const string password = "correct-horse-battery-staple";

        var passwordHash = passwordService.HashPassword(password);

        Assert.NotEqual(password, passwordHash);
        Assert.False(string.IsNullOrEmpty(passwordHash));
    }

    [Fact]
    public void VerifyPassword_ReturnsTrueForCorrectPassword()
    {
        const string password = "correct-horse-battery-staple";
        var passwordHash = passwordService.HashPassword(password);

        var isValid = passwordService.VerifyPassword(password, passwordHash);

        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalseForIncorrectPassword()
    {
        var passwordHash = passwordService.HashPassword("correct-horse-battery-staple");

        var isValid = passwordService.VerifyPassword("incorrect-password", passwordHash);

        Assert.False(isValid);
    }

    [Fact]
    public void DifferentHashesAreProducedForSamePassword()
    {
        const string password = "correct-horse-battery-staple";
        var firstHash = passwordService.HashPassword(password);
        var secondHash = passwordService.HashPassword(password);

        Assert.NotEqual(firstHash, secondHash);
        Assert.True(passwordService.VerifyPassword(password, firstHash));
        Assert.True(passwordService.VerifyPassword(password, secondHash));
    }

    [Theory]
    [InlineData("EXTERNAL_AUTH_NO_PASSWORD")]
    [InlineData("EXTERNAL_AUTH_SOMETHING_ELSE")]
    [InlineData("")]
    [InlineData("   ")]
    public void VerifyPassword_ReturnsFalseForSentinelOrInvalidHashes(string passwordHash)
    {
        var isValid = passwordService.VerifyPassword("AnyPassword123!", passwordHash);

        Assert.False(isValid);
    }
}

#pragma warning restore CA1707