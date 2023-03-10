using BLAZAM.Common.Models.Database.Permissions;
using System.DirectoryServices;
using System.Security;

namespace BLAZAM.Common.Data.ActiveDirectory.Interfaces
{
    public interface IADUser : IGroupableDirectoryModel
    {
        string? City { get; set; }
        string? Company { get; set; }
        string? Department { get; set; }
        string? EmployeeId { get; set; }
        string? GivenName { get; set; }
        string? HomeDirectory { get; set; }
        string? HomeDrive { get; set; }
        string? HomePhone { get; set; }
        string? MiddleName { get; set; }
        DateTime? PasswordLastSet { get; }
        string? PhysicalDeliveryOfficeName { get; set; }
        string? ProfilePath { get; set; }
        string? ScriptPath { get; set; }
        string? Site { get; set; }
        string? State { get; set; }
        string? Street { get; set; }
        string? StreetAddress { get; set; }
        string? Surname { get; set; }
        string? TelephoneNumber { get; set; }
        string? Title { get; set; }
        string? UserPrincipalName { get; set; }
        string? Zip { get; set; }
        List<PrivilegeLevel> PrivilegeLevels { get; set; }
        List<PrivilegeMap> PermissionMappings { get; set; }
        bool HasComputerPrivilege { get; }
        bool HasOUPrivilege { get; }
        bool HasGroupPrivilege { get; }
        bool HasUserPrivilege { get; }
        bool HasCreateUserPrivilege { get; }
        bool HasCreateGroupPrivilege { get; }
        bool HasCreateOUPrivilege { get; }
        byte[]? ThumbnailPhoto { get; set; }
        DateTime? ExpireTime { get; set; }
        SecureString NewPassword { get; set; }

        bool SetPassword(string password);
        bool SetPassword(SecureString password);
        void StagePasswordChange(SecureString newPassword);
    }
}