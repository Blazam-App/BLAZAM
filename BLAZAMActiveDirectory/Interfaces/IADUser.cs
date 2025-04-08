using BLAZAM.ActiveDirectory.Data;
using BLAZAM.Database.Models;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IADUser : IADContact
    {
        string? HomeDirectory { get; set; }
        string? HomeDrive { get; set; }
        string? ProfilePath { get; set; }
        string? ScriptPath { get; set; }
        string? UserPrincipalName { get; set; }
        string? LogOnTo { get; set; }
        LogonHours? LogonHours { get; set; }
        List<FailedADLogonEvent> FailedLogonEvents { get; }
    }
}