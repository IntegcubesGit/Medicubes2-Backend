-- Migration: Rename tenantid columns to orgid (SQL Server)
-- Run this script against your SQL Server database before deploying the updated application.
-- Backup your database before running.

BEGIN TRANSACTION;

-- 1. appuser: has both tenantid and orgid. Backfill orgid from tenantid where null, then drop tenantid.
UPDATE appuser SET orgid = tenantid WHERE orgid IS NULL;
ALTER TABLE appuser ALTER COLUMN orgid INT NOT NULL;
ALTER TABLE appuser DROP COLUMN tenantid;

-- 2. AppRoles
EXEC sp_rename 'AppRoles.TenantId', 'orgid', 'COLUMN';

-- 3. AppMenus
EXEC sp_rename 'AppMenus.TenantId', 'orgid', 'COLUMN';

-- 4. orginfo: PK column
EXEC sp_rename 'orginfo.tenantid', 'orgid', 'COLUMN';

-- 5. orgappsetting
EXEC sp_rename 'orgappsetting.tenantid', 'orgid', 'COLUMN';

-- 6. OrgLocation (if column is TenantId)
EXEC sp_rename 'OrgLocation.TenantId', 'orgid', 'COLUMN';

-- 7. AppUserAppRoles
EXEC sp_rename 'AppUserAppRoles.TenantId', 'orgid', 'COLUMN';

-- 8. configsetting
EXEC sp_rename 'configsetting.tenantid', 'orgid', 'COLUMN';

COMMIT TRANSACTION;
