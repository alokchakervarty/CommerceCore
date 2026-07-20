using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Stores;

/// <summary>
/// The tenant root. Every business-owned row in the system (products, orders,
/// customers, etc.) carries a StoreId back to exactly one Store. A single
/// CommerceCore deployment can host unlimited stores, each with its own
/// catalog, theme, and settings, while sharing the same backend codebase.
/// </summary>
public class Store : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;   // subdomain / URL segment
    public string? Domain { get; set; }                // custom domain, if any
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid DefaultCurrencyId { get; set; }
    public Guid DefaultLanguageId { get; set; }
    public Guid DefaultCountryId { get; set; }

    public StoreSettings? Settings { get; set; }
    public StoreTheme? Theme { get; set; }
}

/// <summary>One-to-one configuration blob for a Store: payment, shipping, email, and SEO settings.
/// Kept separate from Store itself so the hot-path Store row stays small.</summary>
public class StoreSettings : BaseEntity
{
    public Guid StoreId { get; set; }
    public Store? Store { get; set; }

    // Payment
    public string? PaymentProviderName { get; set; }        // "Stripe", "Razorpay", etc.
    public string? PaymentPublicKey { get; set; }
    public string? PaymentSecretKeyEncrypted { get; set; }   // encrypted at rest
    public bool CashOnDeliveryEnabled { get; set; } = true;

    // Shipping
    public bool FreeShippingEnabled { get; set; }
    public decimal? FreeShippingThreshold { get; set; }
    public decimal DefaultFlatShippingRate { get; set; }

    // Email
    public string? SenderEmail { get; set; }
    public string? SenderName { get; set; }
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsernameEncrypted { get; set; }
    public string? SmtpPasswordEncrypted { get; set; }

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? GoogleAnalyticsId { get; set; }
    public string? FacebookPixelId { get; set; }

    // Tax
    public bool PricesIncludeTax { get; set; }
    public Guid? DefaultTaxId { get; set; }
}

/// <summary>One-to-one visual theming for a Store's storefront.</summary>
public class StoreTheme : BaseEntity
{
    public Guid StoreId { get; set; }
    public Store? Store { get; set; }

    public string ThemeName { get; set; } = "default";
    public string PrimaryColor { get; set; } = "#000000";
    public string SecondaryColor { get; set; } = "#FFFFFF";
    public string AccentColor { get; set; } = "#FF6600";
    public string? HeadingFont { get; set; }
    public string? BodyFont { get; set; }
    public string? CustomCss { get; set; }
    public string? LayoutJson { get; set; } // serialized layout/section config for a page builder
}
