using BLAZAM.ActiveDirectory.Adapters;
using System.Text.Json.Serialization;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// Represents Active Directory objects that can be members of groups, such as users, computers, groups, and printers.
    /// </summary>
    /// <remarks>
    /// This interface extends <see cref="IDirectoryEntryAdapter"/> to provide group membership functionality.
    /// </remarks>
    public interface IGroupableDirectoryAdapter : IDirectoryEntryAdapter
    {
        /// <summary>
        /// A description of the directory object.
        /// </summary>
        string? Description { get; set; }

        /// <summary>
        /// The display name of the directory object.
        /// </summary>
        string? DisplayName { get; set; }

        /// <summary>
        /// The email address associated with the directory object, if applicable.
        /// </summary>
        string? Email { get; set; }

        /// <summary>
        /// Indicates whether this object is currently a member of any group.
        /// </summary>
        bool IsAGroupMember { get; }

        /// <summary>
        /// The list of groups that this object is a member of.
        /// </summary>
        [JsonIgnore]
        List<IADGroup> MemberOf { get; }

        /// <summary>
        /// Indicates whether the current web user can assign this object to any groups.
        /// </summary>
        bool CanAssign { get; }

        /// <summary>
        /// Indicates whether the current web user can unassign this object from any groups.
        /// </summary>
        bool CanUnassign { get; }

        /// <summary>
        /// The list of group memberships that are pending removal for this object.
        /// </summary>
        [JsonIgnore]
        List<GroupMembership> ToUnassignFrom { get; }

        /// <summary>
        /// The list of group memberships that are pending assignment for this object.
        /// </summary>
        [JsonIgnore]
        List<GroupMembership> ToAssignTo { get; }

        /// <summary>
        /// Assigns this object to the specified group.
        /// </summary>
        /// <param name="group">The group to assign this object to.</param>
        void AssignTo(IADGroup group);

        /// <summary>
        /// Determines whether this object is a member of the specified group.
        /// </summary>
        /// <param name="group">The group to check membership for.</param>
        /// <returns>True if this object is a member of the group; otherwise, false.</returns>
        bool IsAMemberOf(IADGroup? group);

        /// <summary>
        /// Removes this object from the specified group.
        /// </summary>
        /// <param name="group">The group to remove this object from.</param>
        void UnassignFrom(IADGroup group);
    }
}