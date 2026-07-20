using CommerceCore.Application.Common.Generic;
using CommerceCore.Domain.Entities.Catalog;
using CommerceCore.Domain.Entities.Identity;
using FluentAssertions;
using Xunit;

namespace CommerceCore.Tests.Application.Generic;

public class EntityPropertyCopierTests
{
    [Fact]
    public void CopyScalarProperties_Copies_Simple_Fields_But_Not_Audit_Fields()
    {
        var target = new Brand
        {
            Name = "Old Name",
            Slug = "old-slug",
            IsActive = false
        };
        var originalId = target.Id;
        var originalCreatedDate = target.CreatedDate;

        var source = new Brand
        {
            Name = "New Name",
            Slug = "new-slug",
            IsActive = true,
            Description = "Updated description"
        };

        EntityPropertyCopier.CopyScalarProperties(source, target);

        target.Name.Should().Be("New Name");
        target.Slug.Should().Be("new-slug");
        target.IsActive.Should().BeTrue();
        target.Description.Should().Be("Updated description");

        // Audit/identity fields must never be overwritten by a generic update.
        target.Id.Should().Be(originalId);
        target.CreatedDate.Should().Be(originalCreatedDate);
    }

    [Fact]
    public void CopyScalarProperties_Does_Not_Touch_Navigation_Or_Collection_Properties()
    {
        var target = new Brand { Name = "Target" };
        target.Products.Add(new Product { Name = "Existing Product" });

        var source = new Brand { Name = "Source" };
        // source.Products is an empty collection by default — if the copier touched
        // navigation properties it would wipe target's existing Products collection.

        EntityPropertyCopier.CopyScalarProperties(source, target);

        target.Products.Should().HaveCount(1);
    }
}

public class RefreshTokenTests
{
    [Fact]
    public void IsActive_Should_Be_True_When_Not_Expired_And_Not_Revoked()
    {
        var token = new RefreshToken { ExpiresAt = DateTime.UtcNow.AddDays(1) };
        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_Should_Be_False_When_Expired()
    {
        var token = new RefreshToken { ExpiresAt = DateTime.UtcNow.AddMinutes(-1) };
        token.IsActive.Should().BeFalse();
        token.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsActive_Should_Be_False_When_Revoked_Even_If_Not_Expired()
    {
        var token = new RefreshToken
        {
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            RevokedAt = DateTime.UtcNow
        };
        token.IsActive.Should().BeFalse();
        token.IsRevoked.Should().BeTrue();
    }
}
