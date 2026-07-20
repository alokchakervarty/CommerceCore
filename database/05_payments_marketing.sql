-- ============================================================================
-- PAYMENTS / MARKETING / REVIEWS
-- ============================================================================

CREATE TABLE "Payments" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "OrderId" UUID NOT NULL,
    "Provider" VARCHAR(100) NOT NULL,
    "MethodType" VARCHAR(30) NOT NULL,
    "Status" VARCHAR(30) NOT NULL DEFAULT 'Pending',
    "Amount" DECIMAL(14,2) NOT NULL,
    "CurrencyCode" VARCHAR(10) NOT NULL DEFAULT 'USD',
    "AmountCaptured" DECIMAL(14,2) NOT NULL DEFAULT 0,
    "AmountRefunded" DECIMAL(14,2) NOT NULL DEFAULT 0,
    "GatewayCustomerId" VARCHAR(150) NULL,
    "GatewayPaymentIntentId" VARCHAR(150) NULL,
    "AuthorizedAt" TIMESTAMPTZ NULL,
    "CapturedAt" TIMESTAMPTZ NULL,
    "FailedAt" TIMESTAMPTZ NULL,
    "FailureReason" VARCHAR(500) NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Payments_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Payments_Orders" FOREIGN KEY ("OrderId") REFERENCES "Orders"("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_Payments_OrderId" ON "Payments" ("OrderId");

CREATE TABLE "PaymentTransactions" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "PaymentId" UUID NOT NULL,
    "Type" VARCHAR(30) NOT NULL,
    "Status" VARCHAR(30) NOT NULL,
    "Amount" DECIMAL(14,2) NOT NULL,
    "CurrencyCode" VARCHAR(10) NOT NULL DEFAULT 'USD',
    "GatewayTransactionId" VARCHAR(150) NULL,
    "GatewayResponseRaw" TEXT NULL,
    "FailureReason" VARCHAR(500) NULL,
    "ProcessedAt" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_PaymentTransactions_Payments" FOREIGN KEY ("PaymentId") REFERENCES "Payments"("Id") ON DELETE CASCADE
);

CREATE TABLE "Coupons" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "Code" VARCHAR(50) NOT NULL,
    "Description" VARCHAR(500) NULL,
    "DiscountType" VARCHAR(30) NOT NULL,
    "DiscountValue" DECIMAL(12,2) NOT NULL,
    "MinimumOrderAmount" DECIMAL(12,2) NULL,
    "MaxDiscountAmount" DECIMAL(12,2) NULL,
    "UsageLimitTotal" INTEGER NULL,
    "UsageLimitPerCustomer" INTEGER NULL,
    "TimesUsed" INTEGER NOT NULL DEFAULT 0,
    "RestrictedToCategoryId" UUID NULL,
    "RestrictedToProductId" UUID NULL,
    "RestrictedToCollectionId" UUID NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "StartsAt" TIMESTAMPTZ NULL,
    "EndsAt" TIMESTAMPTZ NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Coupons_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX "UQ_Coupons_StoreId_Code" ON "Coupons" ("StoreId", "Code");

CREATE TABLE "CouponUsage" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "CouponId" UUID NOT NULL,
    "OrderId" UUID NOT NULL,
    "CustomerId" UUID NOT NULL,
    "DiscountAmountApplied" DECIMAL(12,2) NOT NULL,
    "UsedAt" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_CouponUsage_Coupons" FOREIGN KEY ("CouponId") REFERENCES "Coupons"("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_CouponUsage_Coupon_Order" UNIQUE ("CouponId", "OrderId")
);

CREATE TABLE "Wishlists" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "CustomerId" UUID NOT NULL,
    "Name" VARCHAR(150) NOT NULL DEFAULT 'My Wishlist',
    "IsDefault" BOOLEAN NOT NULL DEFAULT true,
    "IsPublic" BOOLEAN NOT NULL DEFAULT false,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Wishlists_Customers" FOREIGN KEY ("CustomerId") REFERENCES "Customers"("Id") ON DELETE CASCADE
);

CREATE TABLE "WishlistItems" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "WishlistId" UUID NOT NULL,
    "ProductId" UUID NOT NULL,
    "ProductVariantId" UUID NULL,
    "AddedAt" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_WishlistItems_Wishlists" FOREIGN KEY ("WishlistId") REFERENCES "Wishlists"("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_WishlistItems" UNIQUE ("WishlistId", "ProductId", "ProductVariantId")
);

CREATE TABLE "Reviews" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "ProductId" UUID NOT NULL,
    "CustomerId" UUID NOT NULL,
    "OrderId" UUID NULL,
    "Rating" INTEGER NOT NULL,
    "Title" VARCHAR(200) NULL,
    "Body" TEXT NULL,
    "IsVerifiedPurchase" BOOLEAN NOT NULL DEFAULT false,
    "IsApproved" BOOLEAN NOT NULL DEFAULT false,
    "ApprovedAt" TIMESTAMPTZ NULL,
    "ApprovedByUserId" UUID NULL,
    "HelpfulCount" INTEGER NOT NULL DEFAULT 0,
    "MerchantReplyBody" TEXT NULL,
    "MerchantRepliedAt" TIMESTAMPTZ NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Reviews_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Reviews_Products" FOREIGN KEY ("ProductId") REFERENCES "Products"("Id") ON DELETE CASCADE,
    CONSTRAINT "CK_Reviews_Rating_Range" CHECK ("Rating" >= 1 AND "Rating" <= 5)
);
CREATE INDEX "IX_Reviews_ProductId" ON "Reviews" ("ProductId");
