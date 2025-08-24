using System.Text.Json.Serialization;
using BLAZAM.ActiveDirectory.Adapters;

namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// Represents an Active Directory group, providing access to group properties and member management.
    /// </summary>
    public interface IADGroup : IGroupableDirectoryAdapter, IIdentityAdapater, IComparable
    {
        /// <summary>
        /// Gets or sets the name of the group associated with this instance.
        /// </summary>
        string? GroupName { get; set; }

        /// <summary>
        /// Gets a value indicating whether the current group contains any members.
        /// </summary>
        bool HasMembers { get; }

        /// <summary>
        /// Gets the list of user members associated with the current group.
        /// </summary>
        /// <remarks>
        /// This property is ignored during JSON serialization due to the <see cref="JsonIgnoreAttribute"/>.
        /// </remarks>
        [JsonIgnore]
        List<IADUser> UserMembers { get; }

        /// <summary>
        /// Gets the list of group members that belong to this group.
        /// </summary>
        /// <remarks>
        /// This property is ignored during JSON serialization due to the <see cref="JsonIgnoreAttribute"/>.
        /// </remarks>
        [JsonIgnore]
        List<IADGroup> GroupMembers { get; }

        /// <summary>
        /// Gets the list of all members of this group, including users, groups, and other directory objects.
        /// </summary>
        /// <remarks>
        /// This property is ignored during JSON serialization due to the <see cref="JsonIgnoreAttribute"/>.
        /// </remarks>
        [JsonIgnore]
        List<IGroupableDirectoryAdapter> Members { get; }

        /// <summary>
        /// Gets a list of member distinguished names or identifiers as strings.
        /// </summary>
        List<string>? MembersAsStrings { get; }

        /// <summary>
        /// Gets an enumerable collection of all nested members, including those in child groups.
        /// </summary>
        /// <remarks>
        /// This property is ignored during JSON serialization due to the <see cref="JsonIgnoreAttribute"/>.
        /// </remarks>
        [JsonIgnore]
        IEnumerable<IGroupableDirectoryAdapter> NestedMembers { get; }

        /// <summary>
        /// Gets the list of group memberships scheduled to be removed from this group.
        /// </summary>
        /// <remarks>
        /// This property is ignored during JSON serialization due to the <see cref="JsonIgnoreAttribute"/>.
        /// </remarks>
        [JsonIgnore]
        List<GroupMembership> MembersToRemove { get; }

        /// <summary>
        /// Gets the list of group memberships scheduled to be added to this group.
        /// </summary>
        /// <remarks>
        /// This property is ignored during JSON serialization due to the <see cref="JsonIgnoreAttribute"/>.
        /// </remarks>
        [JsonIgnore]
        List<GroupMembership> MembersToAdd { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this group is a security group.
        /// </summary>
        bool IsSecurityGroup { get; set; }

        /// <summary>
        /// Gets or sets the scope of the group (Universal, Global, or DomainLocal).
        /// </summary>
        GroupScope GroupScope { get; set; }

        /// <summary>
        /// Assigns a member to this group.
        /// </summary>
        /// <param name="member">The directory adapter representing the member to assign.</param>
        void AssignMember(IGroupableDirectoryAdapter member);

        /// <summary>
        /// Removes a member from this group.
        /// </summary>
        /// <param name="member">The directory adapter representing the member to remove.</param>
        void UnassignMember(IGroupableDirectoryAdapter member);
    }
}