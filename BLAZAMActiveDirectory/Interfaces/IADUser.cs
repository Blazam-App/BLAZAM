using BLAZAM.ActiveDirectory.Data;
using System.Security;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IADUser : IAccountDirectoryAdapter
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
        string? PhysicalDeliveryOfficeName { get; set; }
        string? ProfilePath { get; set; }
        string? ScriptPath { get; set; }
        string? Site { get; set; }
        string? State { get; set; }
        string? POBox { get; set; }
        string? StreetAddress { get; set; }
        string? Surname { get; set; }
        string? TelephoneNumber { get; set; }
        string? Title { get; set; }
        string? UserPrincipalName { get; set; }
        string? Zip { get; set; }


        byte[]? ThumbnailPhoto { get; set; }
        string? LogOnTo { get; set; }
        LogonHours? LogonHours { get; set; }
    }
}