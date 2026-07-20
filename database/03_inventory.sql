-- ============================================================================
-- INVENTORY
-- ============================================================================

CREATE TABLE "Warehouses" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "Code" VARCHAR(50) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "IsDefault" BOOLEAN NOT NULL DEFAULT false,
    "AddressLine1" VARCHAR(255) NULL,
    "AddressLine2" VARCHAR(255) NULL,
    "City" VARCHAR(100) NULL,
    "State" VARCHAR(100) NULL,
    "PostalCode" VARCHAR(20) NULL,
    "CountryId" UUID NULL,
    "ContactPhone" VARCHAR(30) NULL,
    "ContactEmail" VARCHAR(255) NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Warehouses_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX "UQ_Warehouses_StoreId_Code" ON "Warehouses" ("StoreId", "Code");

-- Maps to the domain's InventoryItem entity (renamed only in C# to avoid a class
-- sharing its name with its own namespace); the required table name is "Inventory".
CREATE TABLE "Inventory" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "WarehouseId" UUID NOT NULL,
    "ProductVariantId" UUID NOT NULL,
    "QuantityOnHand" INTEGER NOT NULL DEFAULT 0,
    "QuantityReserved" INTEGER NOT NULL DEFAULT 0,
    "ReorderPoint" INTEGER NOT NULL DEFAULT 0,
    "ReorderQuantity" INTEGER NOT NULL DEFAULT 0,
    "BinLocation" VARCHAR(100) NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Inventory_Warehouses" FOREIGN KEY ("WarehouseId") REFERENCES "Warehouses"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Inventory_Variants" FOREIGN KEY ("ProductVariantId") REFERENCES "ProductVariants"("Id") ON DELETE RESTRICT,
    CONSTRAINT "UQ_Inventory_Warehouse_Variant" UNIQUE ("WarehouseId", "ProductVariantId")
);

CREATE TABLE "StockMovements" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "InventoryItemId" UUID NOT NULL,
    "MovementType" VARCHAR(30) NOT NULL,
    "QuantityChange" INTEGER NOT NULL,
    "QuantityOnHandAfter" INTEGER NOT NULL,
    "ReferenceType" VARCHAR(100) NULL,
    "ReferenceId" UUID NULL,
    "Notes" VARCHAR(1000) NULL,
    "OccurredAt" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_StockMovements_Inventory" FOREIGN KEY ("InventoryItemId") REFERENCES "Inventory"("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_StockMovements_Reference" ON "StockMovements" ("ReferenceType", "ReferenceId");
