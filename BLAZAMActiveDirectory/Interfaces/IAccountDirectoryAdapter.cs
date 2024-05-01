using System.Security;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// Represents Active Directory Account Objects. These types have passwords and can be enabled, disabled,
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

        /// <summary>
        /// If the account has been locked out, this will be the <see cref="DateTime"/> that it was locked
        /// </summary>
        DateTime? LockoutTime { get; }

        /// <summary>
        /// If the account has an expiration this will be the <see cref="DateTime"/> that it will/had expire(d)
        /// </summary>
        DateTime? ExpireTime { get; set; }

        /// <summary>
        /// Collects login data synchronously from all domain controllers in the domain
        /// </summary>
        /// <remarks>
        /// This is a GUI hanging operation and should be surrounded by another thread
        /// </remarks>
        DateTime? LastLogonTime { get; }

        /// <summary>
        /// The time the password was last changed
        /// </summary>
        DateTime? PasswordLastSet { get; }


        /// <summary>
        /// If a password change is staged using <see cref="StagePasswordChange(SecureString, bool)"/>, holds the encrypted new password to be applied.
        /// </summary>
        SecureString? NewPassword { get; set; }
        bool PasswordNotRequired { get; set; }
        bool RequirePasswordChange { get; set; }

        /// <summary>
        /// Changes the password for this entry immediately
        /// </summary>
        /// <param name="password">The new password</param>
        /// <param name="requireChange">Whether to force a password change after reset</param>
        /// <returns>True if the password change was successful, otherwise false.</returns>
        /// <exception cref="ApplicationException"></exception>
        bool SetPassword(SecureString password, bool requireChange = false);


        void StagePasswordChange(SecureString newPassword, bool requireChange = false);
        void StageRequirePasswordChange(bool requireChange);
    }
}