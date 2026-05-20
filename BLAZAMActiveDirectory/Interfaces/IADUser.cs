using BLAZAM.ActiveDirectory.Data;
using BLAZAM.Database.Models;
using BLAZAM.Jobs;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// Represents an Active Directory user account, providing access to user-specific properties and logon information.
    /// </summary>
    /// <remarks>
    /// This interface extends <see cref="IAccountDirectoryAdapter"/> and <see cref="IIdentityAdapater"/>, 
    /// adding properties unique to Active Directory users such as profile paths, logon scripts, and logon events.
    /// </remarks>
    public interface IADUser : IAccountDirectoryAdapter, IIdentityAdapater
    {
        /// <summary>
        /// Gets or sets the path to the user's home directory.
        /// </summary>
        string? HomeDirectory { get; set; }

        /// <summary>
        /// Gets or sets the drive letter assigned to the user's home directory.
        /// </summary>
        string? HomeDrive { get; set; }

        /// <summary>
        /// Gets or sets the path to the user's profile.
        /// </summary>
        string? ProfilePath { get; set; }

        /// <summary>
        /// Gets or sets the path to the user's logon script.
        /// </summary>
        string? ScriptPath { get; set; }

        /// <summary>
        /// Gets or sets the user's principal name (UPN), typically used for logon.
        /// </summary>
        string? UserPrincipalName { get; set; }

        /// <summary>
        /// Gets or sets the list of computers the user is allowed to log on to.
        /// </summary>
        string? LogOnTo { get; set; }

        /// <summary>
        /// Gets or sets the user's allowed logon hours.
        /// </summary>
        LogonHours? LogonHours { get; set; }

        /// <summary>
        /// Gets a list of failed logon events for the user.
        /// </summary>
        List<FailedADLogonEvent> GetFailedLogonEvents(IJob? job=null);
    }
}