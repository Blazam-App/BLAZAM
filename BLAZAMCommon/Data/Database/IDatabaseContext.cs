using BLAZAM.Common.Models.Database;
using BLAZAM.Common.Models.Database.Audit;
using BLAZAM.Common.Models.Database.Permissions;
using BLAZAM.Common.Models.Database.Templates;
using BLAZAM.Common.Models.Database.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

namespace BLAZAM.Common.Data.Database
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
        DbSet<ActionAccessFlag> ObjectActionFlag { get; set; }
        DbSet<OUAuditLog> OUAuditLog { get; set; }
        DbSet<PermissionDelegate> PermissionDelegate { get; set; }
        DbSet<PermissionMap> PermissionMap { get; set; }
        DbSet<PermissionsAuditLog> PermissionsAuditLog { get; set; }
        DbSet<RequestAuditLog> RequestAuditLog { get; set; }
        DbSet<SettingsAuditLog> SettingsAuditLog { get; set; }
        DbSet<SystemAuditLog> SystemAuditLog { get; set; }
        DbSet<UserAuditLog> UserAuditLog { get; set; }
        DbSet<UserSettings> UserSettings { get; set; }


        DatabaseConnectionString? ConnectionString { get; }
        DatabaseException DownReason { get; set; }
        ServiceConnectionState Status { get; }

    }
}