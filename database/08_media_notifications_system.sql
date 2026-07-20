-- ============================================================================
-- MEDIA / NOTIFICATIONS / SYSTEM / AUDIT
-- ============================================================================

CREATE TABLE "Media" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "FileName" VARCHAR(300) NOT NULL,
    "Url" TEXT NOT NULL,
    "ThumbnailUrl" TEXT NULL,
    "ContentType" VARCHAR(150) NOT NULL,
    "FileSizeBytes" BIGINT NOT NULL DEFAULT 0,
    "WidthPx" INTEGER NULL,
    "HeightPx" INTEGER NULL,
    "AltText" VARCHAR(300) NULL,
    "Folder" VARCHAR(150) NULL,
    "UploadedByUserId" UUID NOT NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Media_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE
);

CREATE TABLE "NotificationTemplates" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "Code" VARCHAR(150) NOT NULL,
    "Name" VARCHAR(150) NOT NULL,
    "Channel" VARCHAR(20) NOT NULL,
    "SubjectTemplate" VARCHAR(300) NULL,
    "BodyTemplate" TEXT NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_NotificationTemplates_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_NotificationTemplates" UNIQUE ("StoreId", "Code", "Channel")
);

CREATE TABLE "Notifications" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "NotificationTemplateId" UUID NULL,
    "Channel" VARCHAR(20) NOT NULL,
    "RecipientId" UUID NOT NULL,
    "RecipientAddress" VARCHAR(255) NOT NULL,
    "Subject" VARCHAR(300) NULL,
    "Body" TEXT NOT NULL,
    "IsSent" BOOLEAN NOT NULL DEFAULT false,
    "SentAt" TIMESTAMPTZ NULL,
    "IsRead" BOOLEAN NOT NULL DEFAULT false,
    "ReadAt" TIMESTAMPTZ NULL,
    "FailureReason" VARCHAR(500) NULL,
    "RetryCount" INTEGER NOT NULL DEFAULT 0,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_Notifications_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Notifications_Templates" FOREIGN KEY ("NotificationTemplateId") REFERENCES "NotificationTemplates"("Id") ON DELETE SET NULL
);
CREATE INDEX "IX_Notifications_RecipientId" ON "Notifications" ("RecipientId");

CREATE TABLE "SystemSettings" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NULL,
    "Key" VARCHAR(200) NOT NULL,
    "Value" TEXT NOT NULL,
    "Description" VARCHAR(500) NULL,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "UQ_SystemSettings" UNIQUE ("StoreId", "Key")
);

CREATE TABLE "EmailTemplates" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NOT NULL,
    "Code" VARCHAR(150) NOT NULL,
    "Name" VARCHAR(150) NOT NULL,
    "Subject" VARCHAR(300) NOT NULL,
    "HtmlBody" TEXT NOT NULL,
    "PlainTextBody" TEXT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedDate" TIMESTAMPTZ NOT NULL DEFAULT now(),
    "CreatedBy" UUID NULL, "ModifiedDate" TIMESTAMPTZ NULL, "ModifiedBy" UUID NULL,
    "DeletedDate" TIMESTAMPTZ NULL, "DeletedBy" UUID NULL, "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "Version" INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT "FK_EmailTemplates_Stores" FOREIGN KEY ("StoreId") REFERENCES "Stores"("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_EmailTemplates" UNIQUE ("StoreId", "Code")
);

-- Write-once logs — deliberately NOT derived from the standard BaseEntity audit
-- pattern (an audit log must never itself be audited, soft-deleted, or versioned).
CREATE TABLE "AuditLogs" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NULL,
    "UserId" UUID NULL,
    "UserDisplayName" VARCHAR(200) NULL,
    "EntityName" VARCHAR(150) NOT NULL,
    "EntityId" UUID NOT NULL,
    "Action" VARCHAR(30) NOT NULL,
    "OldValuesJson" TEXT NULL,
    "NewValuesJson" TEXT NULL,
    "ChangedPropertiesJson" TEXT NULL,
    "IpAddress" VARCHAR(50) NULL,
    "UserAgent" VARCHAR(500) NULL,
    "OccurredAt" TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX "IX_AuditLogs_Entity" ON "AuditLogs" ("EntityName", "EntityId");
CREATE INDEX "IX_AuditLogs_OccurredAt" ON "AuditLogs" ("OccurredAt");

CREATE TABLE "ActivityLogs" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "StoreId" UUID NULL,
    "ActorUserId" UUID NULL,
    "ActorDisplayName" VARCHAR(200) NULL,
    "ActivityType" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(1000) NOT NULL,
    "RelatedEntityName" VARCHAR(150) NULL,
    "RelatedEntityId" UUID NULL,
    "OccurredAt" TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX "IX_ActivityLogs_StoreId" ON "ActivityLogs" ("StoreId");
CREATE INDEX "IX_ActivityLogs_OccurredAt" ON "ActivityLogs" ("OccurredAt");
