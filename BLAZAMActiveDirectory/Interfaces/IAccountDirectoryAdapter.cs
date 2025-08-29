using System.Security;
using System.Text.Json.Serialization;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// Represents Active Directory Account Objects. These types have passwords and can be enabled, disabled,
    /// and locked out.
    /// </summary>
    /// <remarks>
    /// Examples include Active Directory users and computers.
    /// </remarks>
    public interface IAccountDirectoryAdapter : IADContact, IIdentityAdapater
    {
        /// <summary>
        /// Indicates whether the current web user can enable this <see cref="IAccountDirectoryAdapter"/>.
        /// </summary>
        bool CanEnable { get; }

        /// <summary>
        /// Indicates whether the current web user can disable this <see cref="IAccountDirectoryAdapter"/>.
        /// </summary>
        bool CanDisable { get; }

        /// <summary>
        /// Indicates whether the current web user can unlock this <see cref="IAccountDirectoryAdapter"/>.
        /// </summary>
        bool CanUnlock { get; }

        /// <summary>
        /// Indicates whether this <see cref="IAccountDirectoryAdapter"/> is currently locked out
        /// of logging in.
        /// </summary>
        bool LockedOut { get; set; }

        /// <summary>
        /// Indicates whether this <see cref="IAccountDirectoryAdapter"/> is disabled.
        /// </summary>
        bool Disabled { get; set; }

        /// <summary>
        /// Indicates whether this <see cref="IAccountDirectoryAdapter"/> is enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Indicates whether the current web user can reset the password for this <see cref="IAccountDirectoryAdapter"/>.
        /// </summary>
        bool CanSetPassword { get; }

        /// <summary>
        /// Indicates whether the current web user can search for disabled <see cref="IAccountDirectoryAdapter"/> objects.
        /// </summary>
        bool CanSearchDisabled { get; }

        /// <summary>
        /// If the account has been locked out, this will be the <see cref="DateTime"/> that it was locked.
        /// </summary>
        DateTime? LockoutTime { get; }

        /// <summary>
        /// If the account has an expiration, this will be the <see cref="DateTime"/> that it will or has expired.
        /// </summary>
        DateTime? ExpireTime { get; set; }

        /// <summary>
        /// Collects login data synchronously from all domain controllers in the domain.
        /// </summary>
        /// <remarks>
        /// This is a GUI hanging operation and should be surrounded by another thread.
        /// </remarks>
        DateTime? LastLogonTime { get; }

        /// <summary>
        /// The time the password was last changed.
        /// </summary>
        DateTime? PasswordLastSet { get; }

        /// <summary>
        /// If a password change is staged using <see cref="StagePasswordChange(SecureString, bool)"/>, holds the encrypted new password to be applied.
        /// </summary>
        [JsonIgnore]
        SecureString? NewPassword { get; set; }

        /// <summary>
        /// Indicates whether a password is not required for this account.
        /// </summary>
        bool PasswordNotRequired { get; set; }

        /// <summary>
        /// Indicates whether the account requires a password change at next logon.
        /// </summary>
        bool RequirePasswordChange { get; set; }

        /// <summary>
        /// The last logon timestamp as replicated across domain controllers.
        /// </summary>
        DateTime? LastLogonTimestamp { get; }

        /// <summary>
        /// The time when the password will expire, if applicable.
        /// </summary>
        DateTime? PasswordExpirationTime { get; }

        /// <summary>
        /// Changes the password for this entry immediately.
        /// </summary>
        /// <param name="password">The new password.</param>
        /// <param name="requireChange">Whether to force a password change after reset.</param>
        /// <returns>True if the password change was successful; otherwise, false.</returns>
        /// <exception cref="AppException">Thrown if the password change fails.</exception>
        bool SetPassword(SecureString password, bool requireChange = false);

        void StageEnable();
        
        /// <summary>
        /// Stages a password change for this account, to be applied later.
        /// </summary>
        /// <param name="newPassword">The new password to stage.</param>
        /// <param name="requireChange">Whether to require a password change at next logon.</param>
        void StagePasswordChange(SecureString newPassword, bool requireChange = false);

        /// <summary>
        /// Stages the requirement for a password change at next logon.
        /// </summary>
        /// <param name="requireChange">Whether to require a password change at next logon.</param>
        void StageRequirePasswordChange(bool requireChange);
    }
}