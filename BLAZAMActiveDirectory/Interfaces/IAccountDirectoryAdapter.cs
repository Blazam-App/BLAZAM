using System.Security;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// Represents Active Dirtory Account Objects. These types have passwords and can be enabled, disabled,
    /// and locked out.
    /// </summary>
    /// <remarks>
    /// Examples include Active Directory users and computers
    /// </remarks>
    public interface IAccountDirectoryAdapter : IGroupableDirectoryAdapter
    {
        /// <summary>
        /// Indicates whether the current web user can enable this <see cref="IAccountDirectoryAdapter"/>
        /// </summary>
        bool CanEnable { get; }
        /// <summary>
        /// Indicates whether the current web user can disable this <see cref="IAccountDirectoryAdapter"/>
        /// </summary>
        bool CanDisable { get; }
        /// <summary>
        /// Indicates whether the current web user can unlock this <see cref="IAccountDirectoryAdapter"/>
        /// </summary>
        bool CanUnlock { get; }
        /// <summary>
        /// Indicates whether this <see cref="IAccountDirectoryAdapter"/> is currently locked out
        /// of logging in
        /// </summary>
        bool LockedOut { get; set; }
        /// <summary>
        /// Indicates whether this <see cref="IAccountDirectoryAdapter"/> is disabled
        /// </summary>
        bool Disabled { get; set; }

        /// <summary>
        /// Indicates whether this <see cref="IAccountDirectoryAdapter"/> is disabled
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Indicates whether the current web user can search disabled <see cref="IAccountDirectoryAdapter"/>'s
        /// </summary>
        bool CanSearchDisabled { get; }
        DateTime? LockoutTime { get; }
        DateTime? ExpireTime { get; set; }
        DateTime? LastLogonTime { get; }
        DateTime? PasswordLastSet { get; }


        /// <summary>
        /// If a password change is staged using <see cref="StagePasswordChange(SecureString, bool)"/>, holds the encrypted new password to be applied.
        /// </summary>
        SecureString? NewPassword { get; set; }

        /// <summary>
        /// Changes the password for this entry immediately
        /// </summary>
        /// <param name="password">The new password</param>
        /// <param name="requireChange">Whether to force a password change after reset</param>
        /// <returns>True if the password change was successful, otherwise false.</returns>
        /// <exception cref="ApplicationException"></exception>
        bool SetPassword(SecureString password, bool requireChange = false);


        void StagePasswordChange(SecureString newPassword, bool requireChange = false);
    }
}