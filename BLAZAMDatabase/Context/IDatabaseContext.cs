﻿using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Database;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Audit;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.Templates;
using BLAZAM.Database.Models.User;
using Microsoft.EntityFrameworkCore;
namespace BLAZAM.Database.Context
{
    public interface IDatabaseContext : IEFCoreDbContext
    {

        DbSet<FieldAccessMapping> AccessLevelFieldMapping { get; set; }
        DbSet<ObjectAccessMapping> AccessLevelObjectMapping { get; set; }
        DbSet<AccessLevel> AccessLevels { get; set; }
        DbSet<ActiveDirectoryField> ActiveDirectoryFields { get; set; }
        DbSet<ADSettings> ActiveDirectorySettings { get; set; }
        DbSet<AppSettings> AppSettings { get; set; }
        DbSet<AuthenticationSettings> AuthenticationSettings { get; set; }
        DbSet<ComputerAuditLog> ComputerAuditLog { get; set; }
        DbSet<DirectoryTemplateFieldValue> DirectoryTemplateFieldValues { get; set; }
        DbSet<DirectoryTemplateGroup> DirectoryTemplateGroups { get; set; }
        DbSet<DirectoryTemplate> DirectoryTemplates { get; set; }
        DbSet<EmailSettings> EmailSettings { get; set; }
        DbSet<EmailTemplate> EmailTemplates { get; set; }
        DbSet<FieldAccessLevel> FieldAccessLevel { get; set; }
        DbSet<GroupAuditLog> GroupAuditLog { get; set; }
        DbSet<LogonAuditLog> LogonAuditLog { get; set; }
        DbSet<ObjectAccessLevel> ObjectAccessLevel { get; set; }
        DbSet<ObjectAction> ObjectActionFlag { get; set; }
        DbSet<OUAuditLog> OUAuditLog { get; set; }
        DbSet<PermissionDelegate> PermissionDelegate { get; set; }
        DbSet<PermissionMapping> PermissionMap { get; set; }
        DbSet<PermissionsAuditLog> PermissionsAuditLog { get; set; }
        DbSet<RequestAuditLog> RequestAuditLog { get; set; }
        DbSet<SettingsAuditLog> SettingsAuditLog { get; set; }
        DbSet<SystemAuditLog> SystemAuditLog { get; set; }
        DbSet<UserAuditLog> UserAuditLog { get; set; }
        DbSet<AppUser> UserSettings { get; set; }


        DatabaseConnectionString? ConnectionString { get; }
        ServiceConnectionState Status { get; }
        DbSet<NotificationMessage> NotificationMessages { get; set; }
        DbSet<UserNotification> UserNotifications { get; set; }
        bool SeedMismatch { get; }
        DbSet<CustomActiveDirectoryField> CustomActiveDirectoryFields { get; set; }

        bool Migrate();
    }
}