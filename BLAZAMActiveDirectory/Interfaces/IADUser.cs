using BLAZAM.ActiveDirectory.Data;
using BLAZAM.Database.Models;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IADUser : IAccountDirectoryAdapter
    {
        string? HomeDirectory { get; set; }
        string? HomeDrive { get; set; }
        string? ProfilePath { get; set; }
        string? ScriptPath { get; set; }
        string? Site { get; set; }
        string? State { get; set; }
        string? POBox { get; set; }
        string? StreetAddress { get; set; }
        string? Sn { get; set; }
        string? TelephoneNumber { get; set; }
        string? Title { get; set; }
        string? UserPrincipalName { get; set; }
        string? LogOnTo { get; set; }
        LogonHours? LogonHours { get; set; }
        List<FailedADLogonEvent> FailedLogonEvents { get; }
    }
}