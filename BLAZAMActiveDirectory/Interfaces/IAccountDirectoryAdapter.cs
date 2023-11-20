namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IAccountDirectoryAdapter : IGroupableDirectoryAdapter
    {
        /// <summary>
        /// Indicates whether the current web user can enable this <see cref="IGroupableDirectoryAdapter"/>
        /// </summary>
        bool CanEnable { get; }
        /// <summary>
        /// Indicates whether the current web user can disable this <see cref="IGroupableDirectoryAdapter"/>
        /// </summary>
        bool CanDisable { get; }
        /// <summary>
        /// Indicates whether the current web user can unlock this <see cref="IGroupableDirectoryAdapter"/>
        /// </summary>
        bool CanUnlock { get; }
        /// <summary>
        /// Indicates whether this <see cref="IGroupableDirectoryAdapter"/> is currently locked out
        /// of logging in
        /// </summary>
        bool LockedOut { get; set; }
        /// <summary>
        /// Indicates whether this <see cref="IGroupableDirectoryAdapter"/> is disabled
        /// </summary>
        bool Disabled { get; set; }

        /// <summary>
        /// Indicates whether this <see cref="IGroupableDirectoryAdapter"/> is disabled
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Indicates whether the current web user can search disabled <see cref="IGroupableDirectoryAdapter"/>'s
        /// </summary>
        bool CanSearchDisabled { get; }
        DateTime? LockoutTime { get; }
        DateTime? ExpireTime { get; set; }
        DateTime? LastLogonTime { get; }
        DateTime? PasswordLastSet { get; }
    }
}