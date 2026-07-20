-- ============================================================================
-- CommerceCore Database Schema (PostgreSQL 15+)
-- ============================================================================
-- This file is the authoritative, hand-maintained reference schema, generated
-- to match the EF Core model exactly. In normal development, use EF Core
-- migrations (`dotnet ef database update`) to apply schema changes — this file
-- exists for: (a) reviewing the full schema in one place without a running
-- database, (b) provisioning environments where the CLI tooling isn't
-- available, and (c) documentation/onboarding.
--
-- Conventions used throughout:
--   - Every table's primary key is a UUID, default-generated via gen_random_uuid().
--   - Every table (except AuditLogs/ActivityLogs, which are intentionally
--     write-once and never derive from BaseEntity) carries the standard audit
--     columns: created_date, created_by, modified_date, modified_by,
--     deleted_date, deleted_by, is_deleted, version.
--   - Soft delete: application code always filters WHERE is_deleted = false;
--     nothing in this schema physically deletes a BaseEntity-derived row.
-- ============================================================================

CREATE EXTENSION IF NOT EXISTS pgcrypto; -- provides gen_random_uuid()

-- Reusable audit-column fragment, spelled out on every CREATE TABLE below
-- (Postgres has no "include macro" for DDL, so this is documentation of the
-- repeated pattern rather than something literally invoked).
--   id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
--   created_date    TIMESTAMPTZ NOT NULL DEFAULT now(),
--   created_by      UUID NULL,
--   modified_date   TIMESTAMPTZ NULL,
--   modified_by     UUID NULL,
--   deleted_date    TIMESTAMPTZ NULL,
--   deleted_by      UUID NULL,
--   is_deleted      BOOLEAN NOT NULL DEFAULT false,
--   version         INTEGER NOT NULL DEFAULT 1

-- ============================================================================
-- STORES (multi-tenant root)
-- ============================================================================

CREATE TABLE "Stores" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" VARCHAR(200) NOT NULL,
    "Slug" VARCHAR(150) NOT NULL,
    "Domain" VARCHAR(255) NULL,
    "Description" TEXT NULL,
    "LogoUrl" TEXT NULL,
    "FaviconUrl" TEXT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "DefaultCurrencyId" UUID NULL,
    "DefaultLanguageId" UUID NULL,
    "DefaultCountryId" UUID NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL,
    "ModifiedDate" TIMESTAMPTZ NULL,
    "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL,
    "DeletedBy" UUID NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "UQ_Stores_Slug" UNIQUE ("Slug"),
    CONSTRAINT "UQ_Stores_Domain" UNIQUE ("Domain")
);

CREATE TABLE "StoreSettings" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "PaymentProviderName" VARCHAR(100) NULL,
    "PaymentPublicKey" TEXT NULL,
    "PaymentSecretKeyEncrypted" TEXT NULL,
    "CashOnDeliveryEnabled" BOOLEAN NOT NULL DEFAULT true,
    "FreeShippingEnabled" BOOLEAN NOT NULL DEFAULT false,
    "FreeShippingThreshold" DECIMAL(12,2) NULL,
    "DefaultFlatShippingRate" DECIMAL(10,2) NOT NULL DEFAULT 0,
    "SenderEmail" VARCHAR(255) NULL,
    "SenderName" VARCHAR(200) NULL,
    "SmtpHost" VARCHAR(255) NULL,
    "SmtpPort" INTEGER NULL,
    "SmtpUsernameEncrypted" TEXT NULL,
    "SmtpPasswordEncrypted" TEXT NULL,
    "MetaTitle" VARCHAR(255) NULL,
    "MetaDescription" VARCHAR(500) NULL,
    "MetaKeywords" VARCHAR(500) NULL,
    "GoogleAnalyticsId" VARCHAR(50) NULL,
    "FacebookPixelId" VARCHAR(50) NULL,
    "PricesIncludeTax" BOOLEAN NOT NULL DEFAULT false,
    "DefaultTaxId" UUID NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_StoreSettings_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_StoreSettings_StoreId" UNIQUE ("StoreId")
);

CREATE TABLE "StoreThemes" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "ThemeName" VARCHAR(100) NOT NULL DEFAULT 'default',
    "PrimaryColor" VARCHAR(20) NOT NULL DEFAULT '#000000',
    "SecondaryColor" VARCHAR(20) NOT NULL DEFAULT '#FFFFFF',
    "AccentColor" VARCHAR(20) NOT NULL DEFAULT '#FF6600',
    "HeadingFont" VARCHAR(100) NULL,
    "BodyFont" VARCHAR(100) NULL,
    "CustomCss" TEXT NULL,
    "LayoutJson" TEXT NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_StoreThemes_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_StoreThemes_StoreId" UNIQUE ("StoreId")
);

-- ============================================================================
-- IDENTITY
-- ============================================================================

CREATE TABLE "Users" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NULL,
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "Email" VARCHAR(255) NOT NULL,
    "EmailConfirmed" BOOLEAN NOT NULL DEFAULT false,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "PhoneNumber" VARCHAR(30) NULL,
    "PhoneNumberConfirmed" BOOLEAN NOT NULL DEFAULT false,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "IsLockedOut" BOOLEAN NOT NULL DEFAULT false,
    "LockoutEndDate" TIMESTAMPTZ NULL,
    "AccessFailedCount" INTEGER NOT NULL DEFAULT 0,
    "LastLoginDate" TIMESTAMPTZ NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Users_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE SET NULL
);
CREATE UNIQUE INDEX "UQ_Users_StoreId_Email" ON "Users" ("StoreId", "Email");

CREATE TABLE "Roles" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(500) NULL,
    "StoreId" UUID NULL,
    "IsSystemRole" BOOLEAN NOT NULL DEFAULT false,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Roles_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX "UQ_Roles_StoreId_Name" ON "Roles" ("StoreId", "Name");

CREATE TABLE "Permissions" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Code" VARCHAR(150) NOT NULL,
    "Name" VARCHAR(150) NOT NULL,
    "Description" VARCHAR(500) NULL,
    "Module" VARCHAR(100) NOT NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "UQ_Permissions_Code" UNIQUE ("Code")
);

CREATE TABLE "RolePermissions" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "RoleId" UUID NOT NULL,
    "PermissionId" UUID NOT NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_RolePermissions_Roles" FOREIGN KEY ("RoleId") REFERENCES "Roles"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_RolePermissions_Permissions" FOREIGN KEY ("PermissionId") REFERENCES "Permissions"("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_RolePermissions" UNIQUE ("RoleId", "PermissionId")
);

CREATE TABLE "UserRoles" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "RoleId" UUID NOT NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_UserRoles_Users" FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserRoles_Roles" FOREIGN KEY ("RoleId") REFERENCES "Roles"("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_UserRoles" UNIQUE ("UserId", "RoleId")
);

CREATE TABLE "RefreshTokens" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "Token" VARCHAR(500) NOT NULL,
    "ExpiresAt" TIMESTAMPTZ NOT NULL,
    "RevokedAt" TIMESTAMPTZ NULL,
    "RevokedByIp" VARCHAR(50) NULL,
    "ReplacedByToken" VARCHAR(500) NULL,
    "CreatedByIp" VARCHAR(50) NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_RefreshTokens_Users" FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_RefreshTokens_Token" UNIQUE ("Token")
);
