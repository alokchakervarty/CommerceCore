-- ============================================================================
-- CATALOG
-- ============================================================================

CREATE TABLE "Categories" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "Slug" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(500) NULL,
    "ImageUrl" TEXT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "DisplayOrder" INTEGER NOT NULL DEFAULT 0,
    "ParentCategoryId" UUID NULL,
    "MetaTitle" VARCHAR(255) NULL,
    "MetaDescription" VARCHAR(500) NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Categories_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Categories_Parent" FOREIGN KEY ("ParentCategoryId") REFERENCES "Categories"("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "UQ_Categories_StoreId_Slug" ON "Categories" ("StoreId", "Slug");

CREATE TABLE "Brands" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "Slug" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(500) NULL,
    "LogoUrl" TEXT NULL,
    "WebsiteUrl" TEXT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Brands_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX "UQ_Brands_StoreId_Slug" ON "Brands" ("StoreId", "Slug");

CREATE TABLE "Collections" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "Slug" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(500) NULL,
    "ImageUrl" TEXT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "StartsAt" TIMESTAMPTZ NULL,
    "EndsAt" TIMESTAMPTZ NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Collections_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX "UQ_Collections_StoreId_Slug" ON "Collections" ("StoreId", "Slug");

CREATE TABLE "Products" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "Name" VARCHAR(300) NOT NULL,
    "Slug" VARCHAR(300) NOT NULL,
    "ShortDescription" VARCHAR(500) NULL,
    "Description" TEXT NULL,
    "Sku" VARCHAR(100) NULL,
    "BasePrice" DECIMAL(12,2) NOT NULL,
    "CompareAtPrice" DECIMAL(12,2) NULL,
    "CostPrice" DECIMAL(12,2) NULL,
    "HasVariants" BOOLEAN NOT NULL DEFAULT false,
    "TrackInventory" BOOLEAN NOT NULL DEFAULT true,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "IsFeatured" BOOLEAN NOT NULL DEFAULT false,
    "WeightKg" DECIMAL(10,3) NULL,
    "LengthCm" DECIMAL(10,2) NULL,
    "WidthCm" DECIMAL(10,2) NULL,
    "HeightCm" DECIMAL(10,2) NULL,
    "CategoryId" UUID NOT NULL,
    "BrandId" UUID NULL,
    "TaxId" UUID NULL,
    "AverageRating" DOUBLE PRECISION NOT NULL DEFAULT 0,
    "ReviewCount" INTEGER NOT NULL DEFAULT 0,
    "MetaTitle" VARCHAR(255) NULL,
    "MetaDescription" VARCHAR(500) NULL,
    "PublishedAt" TIMESTAMPTZ NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Products_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Products_Categories" FOREIGN KEY ("CategoryId") REFERENCES "Categories"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Products_Brands" FOREIGN KEY ("BrandId") REFERENCES "Brands"("Id") ON DELETE SET NULL
);
CREATE UNIQUE INDEX "UQ_Products_StoreId_Slug" ON "Products" ("StoreId", "Slug");
CREATE INDEX "IX_Products_StoreId_Sku" ON "Products" ("StoreId", "Sku");
CREATE INDEX "IX_Products_CategoryId" ON "Products" ("CategoryId");

CREATE TABLE "CollectionProducts" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "CollectionId" UUID NOT NULL,
    "ProductId" UUID NOT NULL,
    "DisplayOrder" INTEGER NOT NULL DEFAULT 0,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_CollectionProducts_Collections" FOREIGN KEY ("CollectionId") REFERENCES "Collections"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_CollectionProducts_Products" FOREIGN KEY ("ProductId") REFERENCES "Products"("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_CollectionProducts" UNIQUE ("CollectionId", "ProductId")
);

CREATE TABLE "ProductVariants" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProductId" UUID NOT NULL,
    "Sku" VARCHAR(100) NOT NULL,
    "Barcode" VARCHAR(100) NULL,
    "Price" DECIMAL(12,2) NULL,
    "CompareAtPrice" DECIMAL(12,2) NULL,
    "WeightKg" DECIMAL(10,3) NULL,
    "ImageUrl" TEXT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "IsDefault" BOOLEAN NOT NULL DEFAULT false,
    "DisplayName" VARCHAR(300) NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_ProductVariants_Products" FOREIGN KEY ("ProductId") REFERENCES "Products"("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_ProductVariants_Sku" UNIQUE ("Sku")
);

CREATE TABLE "ProductImages" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProductId" UUID NOT NULL,
    "ProductVariantId" UUID NULL,
    "Url" TEXT NOT NULL,
    "AltText" VARCHAR(300) NULL,
    "DisplayOrder" INTEGER NOT NULL DEFAULT 0,
    "IsPrimary" BOOLEAN NOT NULL DEFAULT false,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_ProductImages_Products" FOREIGN KEY ("ProductId") REFERENCES "Products"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ProductImages_Variants" FOREIGN KEY ("ProductVariantId") REFERENCES "ProductVariants"("Id") ON DELETE SET NULL
);

-- The dynamic attribute engine: same three tables serve every vertical.
CREATE TABLE "Attributes" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "Name" VARCHAR(150) NOT NULL,
    "Code" VARCHAR(150) NOT NULL,
    "InputType" VARCHAR(30) NOT NULL DEFAULT 'Text',
    "IsVariantDimension" BOOLEAN NOT NULL DEFAULT false,
    "IsFilterable" BOOLEAN NOT NULL DEFAULT false,
    "IsRequired" BOOLEAN NOT NULL DEFAULT false,
    "DisplayOrder" INTEGER NOT NULL DEFAULT 0,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Attributes_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX "UQ_Attributes_StoreId_Code" ON "Attributes" ("StoreId", "Code");

CREATE TABLE "AttributeValues" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "AttributeId" UUID NOT NULL,
    "Value" VARCHAR(200) NOT NULL,
    "ColorHex" VARCHAR(20) NULL,
    "DisplayOrder" INTEGER NOT NULL DEFAULT 0,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_AttributeValues_Attributes" FOREIGN KEY ("AttributeId") REFERENCES "Attributes"("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_AttributeValues" UNIQUE ("AttributeId", "Value")
);

CREATE TABLE "ProductAttributes" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProductId" UUID NULL,
    "ProductVariantId" UUID NULL,
    "AttributeId" UUID NOT NULL,
    "AttributeValueId" UUID NULL,
    "FreeTextValue" VARCHAR(500) NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_ProductAttributes_Products" FOREIGN KEY ("ProductId") REFERENCES "Products"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ProductAttributes_Variants" FOREIGN KEY ("ProductVariantId") REFERENCES "ProductVariants"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ProductAttributes_Attributes" FOREIGN KEY ("AttributeId") REFERENCES "Attributes"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_ProductAttributes_AttributeValues" FOREIGN KEY ("AttributeValueId") REFERENCES "AttributeValues"("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_ProductAttribute_ExactlyOneOwner" CHECK (
        ("ProductId" IS NOT NULL AND "ProductVariantId" IS NULL) OR
        ("ProductId" IS NULL AND "ProductVariantId" IS NOT NULL)
    )
);
