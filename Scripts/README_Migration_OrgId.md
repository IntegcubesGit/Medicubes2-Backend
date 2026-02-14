# Migrations (PostgreSQL only)

## 1. Rename tenant columns to orgid

**Script:** `RenameTenantIdToOrgId_PostgreSQL.sql`

- `tenantid` → `orgid` (all relevant tables)
- **org_info (orginfo):** `tenantcode` → `code`, `tenantname` → `name`

Run this first.

## 2. Rename tables

**Script:** `RenameTables_PostgreSQL.sql`

| Old name       | New name        |
|----------------|-----------------|
| appuser        | app_user        |
| AppRoles       | app_role        |
| AppMenus       | app_menu        |
| OrgLocation    | org_location    |
| AppRoleAppMenus| app_rolemenu    |
| UserRoles      | app_userrole    |
| appuserlocation| app_userlocation|
| appuserstaff   | app_userstaff   |
| configsetting  | app_config      |
| orgappsetting  | org_appsetting  |
| orginfo        | org_info        |

Run after the column-rename script. If your actual table names are all lowercase (e.g. `approles` instead of `"AppRoles"`), edit the script to match.

**Before running any script:** Backup your database and ensure no application is writing during the migration.

**After running both:** Deploy the updated application.
