using CommerceCore.Application.Features.Auth.Commands;
using CommerceCore.Application.Features.Catalog.Products;
using FluentAssertions;
using Xunit;

namespace CommerceCore.Tests.Application.Validators;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Should_Fail_When_Email_Is_Invalid()
    {
        var command = new RegisterCommand("Jane", "Doe", "not-an-email", "Password1", null);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Email));
    }

    [Theory]
    [InlineData("short1A")]     // too short
    [InlineData("alllowercase1")] // no uppercase
    [InlineData("NoDigitsHere")]  // no digit
    public void Should_Fail_For_Weak_Passwords(string password)
    {
        var command = new RegisterCommand("Jane", "Doe", "jane@example.com", password, null);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_Pass_For_Valid_Registration()
    {
        var command = new RegisterCommand("Jane", "Doe", "jane@example.com", "Password1", "+15551234567");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void Should_Fail_When_Price_Is_Zero_Or_Negative()
    {
        var command = new CreateProductCommand(
            "Test Product", null, null, null, 0m, null, null, true, Guid.NewGuid(), null, null);

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductCommand.BasePrice));
    }

    [Fact]
    public void Should_Fail_When_Name_Is_Empty()
    {
        var command = new CreateProductCommand(
            "", null, null, null, 19.99m, null, null, true, Guid.NewGuid(), null, null);

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_Pass_For_Valid_Product()
    {
        var command = new CreateProductCommand(
            "Midnight Oud Perfume", "A rich, woody fragrance", "Full description here", "OUD-001",
            49.99m, 59.99m, 20.00m, true, Guid.NewGuid(), null, new[] { "https://example.com/image.jpg" });

        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
