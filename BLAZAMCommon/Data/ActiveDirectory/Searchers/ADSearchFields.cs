namespace BLAZAM.Common.Data.ActiveDirectory.Searchers
{
    public class ADSearchFields
    {
        public string? SamAccountName { get; set; }

        /// <summary>
        /// The ADS long value to search for locked out users from"
        /// </summary>
        /// <remarks>
        /// To find all locked out entries, use 1
        /// </remarks>
        public long? LockoutTime { get; set; }

        public string? SID { get; set; }

        public string? DN { get; set; }

        public DateTime? Created { get; set; }

        public string? Changed { get; set; }

        public string? PasswordLastSet { get; set; }

        public string? CN { get; set; }

        public string? MemberOf { get; set; }
    }
}