﻿using System.DirectoryServices;

namespace BLAZAM.ActiveDirectory.Interfaces
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
        /// Indicates whether the current web user can unassign to/from this <see cref="IGroupableDirectoryAdapter"/>
        /// </summary>
        bool CanUnassign { get; }
      

        void AssignTo(IADGroup group);
        bool IsAMemberOf(IADGroup group);
        void UnassignFrom(IADGroup group);
    }
}