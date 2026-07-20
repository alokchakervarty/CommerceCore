-- ============================================================================
-- REFERENCE DATA (platform-wide, not store-scoped)
-- ============================================================================

CREATE TABLE "Countries" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" VARCHAR(150) NOT NULL,
    "Iso2Code" VARCHAR(2) NOT NULL,
    "Iso3Code" VARCHAR(3) NOT NULL,
    "PhoneCode" VARCHAR(10) NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "UQ_Countries_Iso2" UNIQUE ("Iso2Code")
);

CREATE TABLE "States" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "CountryId" UUID NOT NULL,
    "Name" VARCHAR(150) NOT NULL,
    "Code" VARCHAR(20) NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_States_Countries" FOREIGN KEY ("CountryId") REFERENCES "Countries"("Id") ON DELETE RESTRICT
);
CREATE INDEX "IX_States_CountryId_Name" ON "States" ("CountryId", "Name");

CREATE TABLE "Cities" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StateId" UUID NOT NULL,
    "Name" VARCHAR(150) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Cities_States" FOREIGN KEY ("StateId") REFERENCES "States"("Id") ON DELETE RESTRICT
);
CREATE INDEX "IX_Cities_StateId_Name" ON "Cities" ("StateId", "Name");

CREATE TABLE "Currencies" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Code" VARCHAR(3) NOT NULL,
    "Name" VARCHAR(100) NOT NULL,
    "Symbol" VARCHAR(10) NOT NULL,
    "DecimalPlaces" INTEGER NOT NULL DEFAULT 2,
    "ExchangeRateToBase" DECIMAL(18,6) NOT NULL DEFAULT 1,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "UQ_Currencies_Code" UNIQUE ("Code")
);

CREATE TABLE "Languages" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Code" VARCHAR(10) NOT NULL,
    "Name" VARCHAR(100) NOT NULL,
    "NativeName" VARCHAR(100) NOT NULL,
    "IsRtl" BOOLEAN NOT NULL DEFAULT false,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "UQ_Languages_Code" UNIQUE ("Code")
);

CREATE TABLE "Taxes" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "Name" VARCHAR(150) NOT NULL,
    "RatePercentage" DECIMAL(6,3) NOT NULL,
    "CountryId" UUID NULL,
    "StateId" UUID NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "Priority" INTEGER NOT NULL DEFAULT 0,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Taxes_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_Taxes_StoreId" ON "Taxes" ("StoreId");

CREATE TABLE "ShippingZones" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "Name" VARCHAR(150) NOT NULL,
    "CountryId" UUID NULL,
    "StateId" UUID NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_ShippingZones_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE
);

CREATE TABLE "ShippingMethods" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "ShippingZoneId" UUID NOT NULL,
    "Name" VARCHAR(150) NOT NULL,
    "Description" VARCHAR(500) NULL,
    "RateType" VARCHAR(30) NOT NULL DEFAULT 'Flat',
    "FlatRate" DECIMAL(10,2) NOT NULL DEFAULT 0,
    "RatePerKg" DECIMAL(10,2) NULL,
    "FreeShippingThreshold" DECIMAL(10,2) NULL,
    "EstimatedDaysMin" INTEGER NOT NULL DEFAULT 0,
    "EstimatedDaysMax" INTEGER NOT NULL DEFAULT 0,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "DisplayOrder" INTEGER NOT NULL DEFAULT 0,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_ShippingMethods_Zones" FOREIGN KEY ("ShippingZoneId") REFERENCES "ShippingZones"("Id") ON DELETE CASCADE
);
