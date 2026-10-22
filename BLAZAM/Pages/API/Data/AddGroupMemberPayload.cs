namespace BLAZAM.Pages.API.Data
{
    /// <summary>
    /// Request payload for adding or removing a member from a group.
    /// </summary>
    public class AddGroupMemberPayload
    {
        /// <summary>
        /// The member identifier (SID, DN, or SAMAccountName) to add or remove.
        /// </summary>
        public string MemberIdentifier { get; set; }
    }
}
