using System.DirectoryServices;

namespace BLAZAM.Common.Data.ActiveDirectory.Interfaces
{
    public interface IGroupableDirectoryAdapter : IDirectoryEntryAdapter
    {
        string? Description { get; set; }
        string? DisplayName { get; set; }
        string? Email { get; set; }
        bool IsAGroupMember { get; }
        List<IADGroup> MemberOf { get; }
        /// <summary>
        /// Indicates whether the current web user can assign this <see cref="IGroupableDirectoryAdapter"/> to any groups
        /// </summary>
        bool CanAssign { get; }
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

        void AssignTo(IADGroup group);
        bool IsAMemberOf(IADGroup group);
        void UnassignFrom(IADGroup group);
    }
}