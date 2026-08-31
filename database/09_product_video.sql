-- Apply this migration to databases created before VideoUrl was added to Products.
ALTER TABLE "Products"
    ADD COLUMN IF NOT EXISTS "VideoUrl" VARCHAR(1000) NULL;