using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Helpers;

namespace BLAZAM.ActiveDirectory.Searchers
{
    public class ADSearchFields
    {
        private string? samAccountName;
        private string? sid;
        private byte[]? guid;

        private string? dN;
        private string? cN;
        private string? memberOf;
        private string bitLockerRecoveryId;

        public string? SamAccountName { get => samAccountName; set => samAccountName = value.EscapeLdapSearchFilter(); }

        /// <summary>
        /// The ADS long value to search for locked out users from"
        /// </summary>
        /// <remarks>
        /// To find all locked out entries, use 1
        /// </remarks>
        public long? LockoutTime { get; set; }


        public long? LastLogonTime { get; set; }

        public string? SID { get => sid; set => sid = value.EscapeLdapSearchFilter(); }
        public byte[]? GUID { get => guid; set => guid = value; }
        public string? DN { get => dN; set => dN = value.EscapeLdapSearchFilter(); }

        public DateTime? Created { get; set; }

        public DateTime? Changed { get; set; }

        public DateTime? PasswordLastSet { get; set; }
        public DateTime? AccountExpires { get; set; }

        public string? CN { get => cN; set => cN = value; }

        public string? MemberOf { get => memberOf; set => memberOf = value; }
        public IADGroup? NestedMemberOf { get; internal set; }
        public string BitLockerRecoveryId { get => bitLockerRecoveryId; internal set => bitLockerRecoveryId = value.EscapeLdapSearchFilter(); }
    }

}