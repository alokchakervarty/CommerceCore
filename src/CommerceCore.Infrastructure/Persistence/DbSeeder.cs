using CommerceCore.Domain.Entities.Identity;
using CommerceCore.Domain.Entities.Reference;
using CommerceCore.Domain.Entities.Stores;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Infrastructure.Persistence;

/// <summary>
/// Seeds the minimum data needed to actually use the API on a fresh database: one
/// Store (every request needs an X-Store-Id header to resolve against), its Admin/
/// Customer roles, a seeded Admin user, and a small set of reference Countries so
/// Address/Checkout flows have something valid to point at.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Countries.AnyAsync())
        {
            db.Countries.AddRange(
                new Country { Name = "United States", Iso2Code = "US", Iso3Code = "USA", PhoneCode = "+1" },
                new Country { Name = "United Kingdom", Iso2Code = "GB", Iso3Code = "GBR", PhoneCode = "+44" },
                new Country { Name = "India", Iso2Code = "IN", Iso3Code = "IND", PhoneCode = "+91" },
                new Country { Name = "Canada", Iso2Code = "CA", Iso3Code = "CAN", PhoneCode = "+1" },
                new Country { Name = "Australia", Iso2Code = "AU", Iso3Code = "AUS", PhoneCode = "+61" });
            await db.SaveChangesAsync();
        }

        var store = await db.Stores.FirstOrDefaultAsync(s => s.Slug == "default");
        if (store == null)
        {
            var defaultCountry = await db.Countries.FirstAsync(c => c.Iso2Code == "US");

            store = new Store
            {
                Name = "Default Store",
                Slug = "default",
                Description = "The default seeded store. Create additional Store rows for more tenants — the same backend serves them all.",
                IsActive = true,
                DefaultCountryId = defaultCountry.Id
            };
            db.Stores.Add(store);
            await db.SaveChangesAsync();

            db.StoreSettings.Add(new StoreSettings
            {
                StoreId = store.Id,
                CashOnDeliveryEnabled = true,
                SenderName = "Default Store",
                MetaTitle = "Default Store"
            });

            db.StoreThemes.Add(new StoreTheme
            {
                StoreId = store.Id,
                ThemeName = "default"
            });

            await db.SaveChangesAsync();
        }

        var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.StoreId == store.Id && r.Name == "Admin");
        if (adminRole == null)
        {
            adminRole = new Role { StoreId = store.Id, Name = "Admin", Description = "Full store administrator access.", IsSystemRole = true };
            db.Roles.Add(adminRole);
        }

        var customerRole = await db.Roles.FirstOrDefaultAsync(r => r.StoreId == store.Id && r.Name == "Customer");
        if (customerRole == null)
        {
            customerRole = new Role { StoreId = store.Id, Name = "Customer", Description = "Default role for storefront shoppers.", IsSystemRole = true };
            db.Roles.Add(customerRole);
        }

        await db.SaveChangesAsync();

        var adminExists = await db.Users.AnyAsync(u => u.StoreId == store.Id && u.Email == "admin@example.com");
        if (!adminExists)
        {
            var admin = new User
            {
                StoreId = store.Id,
                FirstName = "Store",
                LastName = "Admin",
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 12),
                EmailConfirmed = true,
                IsActive = true
            };
            db.Users.Add(admin);
            await db.SaveChangesAsync();

            db.UserRoles.Add(new UserRole { UserId = admin.Id, RoleId = adminRole.Id });
            await db.SaveChangesAsync();
        }
    }
}
