-- Migration: Rename tables to new naming convention (PostgreSQL)
-- Run AFTER RenameTenantIdToOrgId_PostgreSQL.sql (or ensure column renames are done).
-- Backup your database before running.
--
-- If you get "relation does not exist": your DB may use different casing.
-- Try the "All lowercase" block at the end instead (comment out this block, uncomment that one).

BEGIN;

-- appuser -> app_user
ALTER TABLE appuser RENAME TO app_user;

-- AppRoles -> app_role
ALTER TABLE "AppRoles" RENAME TO app_role;

-- AppMenus -> app_menu
ALTER TABLE "AppMenus" RENAME TO app_menu;

-- OrgLocation -> org_location
ALTER TABLE "OrgLocation" RENAME TO org_location;

-- AppRoleAppMenus -> app_rolemenu
ALTER TABLE "AppRoleAppMenus" RENAME TO app_rolemenu;

-- UserRoles -> app_userrole
ALTER TABLE "UserRoles" RENAME TO app_userrole;

-- appuserlocation -> app_userlocation
ALTER TABLE appuserlocation RENAME TO app_userlocation;

-- appuserstaff -> app_userstaff
ALTER TABLE appuserstaff RENAME TO app_userstaff;

-- configsetting -> app_config
ALTER TABLE configsetting RENAME TO app_config;

-- orgappsetting -> org_appsetting
ALTER TABLE orgappsetting RENAME TO org_appsetting;

-- orginfo -> org_info
ALTER TABLE orginfo RENAME TO org_info;

COMMIT;

-- ========== All lowercase (if your tables are e.g. approles, appmenus) ==========
-- BEGIN;
-- ALTER TABLE appuser RENAME TO app_user;
-- ALTER TABLE approles RENAME TO app_role;
-- ALTER TABLE appmenus RENAME TO app_menu;
-- ALTER TABLE orglocation RENAME TO org_location;
-- ALTER TABLE approleappmenus RENAME TO app_rolemenu;
-- ALTER TABLE userroles RENAME TO app_userrole;
-- ALTER TABLE appuserlocation RENAME TO app_userlocation;
-- ALTER TABLE appuserstaff RENAME TO app_userstaff;
-- ALTER TABLE configsetting RENAME TO app_config;
-- ALTER TABLE orgappsetting RENAME TO org_appsetting;
-- ALTER TABLE orginfo RENAME TO org_info;
-- COMMIT;
