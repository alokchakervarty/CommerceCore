-- ============================================================================
-- SEED DATA
-- ============================================================================
-- Minimum data needed to actually use the API on a fresh database. The same
-- seeding also happens automatically via DbSeeder.cs when the Api project
-- starts in the Development environment — this script is provided for
-- environments provisioned directly from these SQL files instead.

INSERT INTO "Countries" ("Id", "Name", "Iso2Code", "Iso3Code", "PhoneCode")
VALUES
    (gen_random_uuid(), 'United States', 'US', 'USA', '+1'),
    (gen_random_uuid(), 'United Kingdom', 'GB', 'GBR', '+44'),
    (gen_random_uuid(), 'India', 'IN', 'IND', '+91'),
    (gen_random_uuid(), 'Canada', 'CA', 'CAN', '+1'),
    (gen_random_uuid(), 'Australia', 'AU', 'AUS', '+61')
ON CONFLICT ("Iso2Code") DO NOTHING;

INSERT INTO "Currencies" ("Id", "Code", "Name", "Symbol", "DecimalPlaces")
VALUES
    (gen_random_uuid(), 'USD', 'US Dollar', '$', 2),
    (gen_random_uuid(), 'GBP', 'British Pound', '£', 2),
    (gen_random_uuid(), 'INR', 'Indian Rupee', '₹', 2),
    (gen_random_uuid(), 'EUR', 'Euro', '€', 2)
ON CONFLICT ("Code") DO NOTHING;

INSERT INTO "Languages" ("Id", "Code", "Name", "NativeName", "IsRtl")
VALUES
    (gen_random_uuid(), 'en', 'English', 'English', false),
    (gen_random_uuid(), 'hi', 'Hindi', 'हिन्दी', false),
    (gen_random_uuid(), 'ar', 'Arabic', 'العربية', true)
ON CONFLICT ("Code") DO NOTHING;

-- Default Store + Admin user (password: Admin@123 — CHANGE THIS before any real deployment).
-- The BCrypt hash below corresponds to "Admin@123" at work factor 12.
DO $$
DECLARE
    v_store_id UUID;
    v_us_country_id UUID;
    v_admin_role_id UUID;
    v_customer_role_id UUID;
BEGIN
    SELECT "Id" INTO v_us_country_id FROM "Countries" WHERE "Iso2Code" = 'US';

    IF NOT EXISTS (SELECT 1 FROM "Stores" WHERE "Slug" = 'default') THEN
        INSERT INTO "Stores" ("Id", "Name", "Slug", "Description", "IsActive", "DefaultCountryId")
        VALUES (gen_random_uuid(), 'Default Store', 'default',
                'The default seeded store. Create additional Store rows for more tenants.',
                true, v_us_country_id)
        RETURNING "Id" INTO v_store_id;

        INSERT INTO "StoreSettings" ("Id", "StoreId", "CashOnDeliveryEnabled", "SenderName", "MetaTitle")
        VALUES (gen_random_uuid(), v_store_id, true, 'Default Store', 'Default Store');

        INSERT INTO "StoreThemes" ("Id", "StoreId", "ThemeName")
        VALUES (gen_random_uuid(), v_store_id, 'default');

        INSERT INTO "Roles" ("Id", "StoreId", "Name", "Description", "IsSystemRole")
        VALUES (gen_random_uuid(), v_store_id, 'Admin', 'Full store administrator access.', true)
        RETURNING "Id" INTO v_admin_role_id;

        INSERT INTO "Roles" ("Id", "StoreId", "Name", "Description", "IsSystemRole")
        VALUES (gen_random_uuid(), v_store_id, 'Customer', 'Default role for storefront shoppers.', true)
        RETURNING "Id" INTO v_customer_role_id;

        INSERT INTO "Users" ("Id", "StoreId", "FirstName", "LastName", "Email", "PasswordHash", "EmailConfirmed", "IsActive")
        VALUES (
            gen_random_uuid(), v_store_id, 'Store', 'Admin', 'admin@example.com',
            '$2b$12$NP7IHlnUjpdxxHPRWt37euRmqreBeXB.FqdBQ2QTIAI32PyFR.wnO', -- verified BCrypt hash of "Admin@123", work factor 12
            true, true
        );

        INSERT INTO "UserRoles" ("Id", "UserId", "RoleId")
        SELECT gen_random_uuid(), u."Id", v_admin_role_id
        FROM "Users" u WHERE u."StoreId" = v_store_id AND u."Email" = 'admin@example.com';
    END IF;
END $$;
