namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IIdentityAdapater : IGroupableDirectoryAdapter
    {
        /// <summary>
        /// The SAMAccountName property, generally used as the username property
        /// </summary>
        string? SAMAccountName { get; set; }

    }
}
