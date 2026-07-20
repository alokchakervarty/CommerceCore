-- ============================================================================
-- CMS
-- ============================================================================

CREATE TABLE "BlogCategories" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "Name" VARCHAR(150) NOT NULL,
    "Slug" VARCHAR(150) NOT NULL,
    "Description" VARCHAR(500) NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_BlogCategories_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX "UQ_BlogCategories_StoreId_Slug" ON "BlogCategories" ("StoreId", "Slug");

CREATE TABLE "Blogs" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "BlogCategoryId" UUID NULL,
    "Title" VARCHAR(300) NOT NULL,
    "Slug" VARCHAR(300) NOT NULL,
    "Excerpt" VARCHAR(500) NULL,
    "Body" TEXT NOT NULL,
    "FeaturedImageUrl" TEXT NULL,
    "AuthorUserId" UUID NOT NULL,
    "IsPublished" BOOLEAN NOT NULL DEFAULT false,
    "PublishedAt" TIMESTAMPTZ NULL,
    "MetaTitle" VARCHAR(255) NULL,
    "MetaDescription" VARCHAR(500) NULL,
    "ViewCount" INTEGER NOT NULL DEFAULT 0,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Blogs_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Blogs_BlogCategories" FOREIGN KEY ("BlogCategoryId") REFERENCES "BlogCategories"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Blogs_Users" FOREIGN KEY ("AuthorUserId") REFERENCES "Users"("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "UQ_Blogs_StoreId_Slug" ON "Blogs" ("StoreId", "Slug");

CREATE TABLE "Pages" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "Title" VARCHAR(300) NOT NULL,
    "Slug" VARCHAR(300) NOT NULL,
    "Body" TEXT NOT NULL,
    "IsPublished" BOOLEAN NOT NULL DEFAULT true,
    "MetaTitle" VARCHAR(255) NULL,
    "MetaDescription" VARCHAR(500) NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Pages_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX "UQ_Pages_StoreId_Slug" ON "Pages" ("StoreId", "Slug");

CREATE TABLE "Menus" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "Name" VARCHAR(150) NOT NULL,
    "Location" VARCHAR(100) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Menus_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE
);

CREATE TABLE "MenuItems" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "MenuId" UUID NOT NULL,
    "ParentMenuItemId" UUID NULL,
    "Label" VARCHAR(150) NOT NULL,
    "LinkType" VARCHAR(50) NOT NULL DEFAULT 'External',
    "LinkTarget" VARCHAR(500) NOT NULL,
    "OpenInNewTab" BOOLEAN NOT NULL DEFAULT false,
    "DisplayOrder" INTEGER NOT NULL DEFAULT 0,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_MenuItems_Menus" FOREIGN KEY ("MenuId") REFERENCES "Menus"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_MenuItems_Parent" FOREIGN KEY ("ParentMenuItemId") REFERENCES "MenuItems"("Id") ON DELETE RESTRICT
);

CREATE TABLE "Banners" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "Title" VARCHAR(200) NOT NULL,
    "Subtitle" VARCHAR(300) NULL,
    "ImageUrl" TEXT NOT NULL,
    "MobileImageUrl" TEXT NULL,
    "LinkType" VARCHAR(50) NULL,
    "LinkTarget" VARCHAR(500) NULL,
    "Placement" VARCHAR(100) NOT NULL DEFAULT 'homepage-hero',
    "DisplayOrder" INTEGER NOT NULL DEFAULT 0,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "StartsAt" TIMESTAMPTZ NULL,
    "EndsAt" TIMESTAMPTZ NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Banners_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE
);

CREATE TABLE "Faqs" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "Question" VARCHAR(500) NOT NULL,
    "Answer" TEXT NOT NULL,
    "Category" VARCHAR(100) NULL,
    "DisplayOrder" INTEGER NOT NULL DEFAULT 0,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Faqs_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE
);
