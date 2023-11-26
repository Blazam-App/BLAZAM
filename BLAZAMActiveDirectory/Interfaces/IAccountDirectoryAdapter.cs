using System.Security;

namespace BLAZAM.ActiveDirectory.Interfaces
{
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

        bool SetPassword(SecureString password, bool requireChange = false);
        void StagePasswordChange(SecureString newPassword, bool requireChange = false);
    }
}