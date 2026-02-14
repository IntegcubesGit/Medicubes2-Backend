-- Migration: Rename tenantid columns to orgid (PostgreSQL)
-- Run this script against your Postgres database before deploying the updated application.
-- Backup your database before running.

BEGIN;

-- 1. appuser: backfill orgid from tenantid where null, then drop tenantid
UPDATE appuser SET orgid = tenantid WHERE orgid IS NULL;
ALTER TABLE appuser ALTER COLUMN orgid SET NOT NULL;
ALTER TABLE appuser DROP COLUMN IF EXISTS tenantid;

-- 2. AppRoles
ALTER TABLE "AppRoles" RENAME COLUMN "TenantId" TO "orgid";

-- 3. AppMenus
ALTER TABLE "AppMenus" RENAME COLUMN "TenantId" TO "orgid";

-- 4. orginfo: PK and column renames
ALTER TABLE orginfo RENAME COLUMN tenantid TO orgid;
ALTER TABLE orginfo RENAME COLUMN tenantcode TO code;
ALTER TABLE orginfo RENAME COLUMN tenantname TO name;

-- 5. orgappsetting
ALTER TABLE orgappsetting RENAME COLUMN tenantid TO orgid;

-- 6. OrgLocation
ALTER TABLE "OrgLocation" RENAME COLUMN "TenantId" TO "orgid";

-- 7. UserRoles (Identity user-role table, mapped in DbContext as "UserRoles")
ALTER TABLE "UserRoles" RENAME COLUMN "TenantId" TO "orgid";

-- 8. configsetting
ALTER TABLE configsetting RENAME COLUMN tenantid TO orgid;

COMMIT;
