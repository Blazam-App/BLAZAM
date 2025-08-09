namespace BLAZAM.ActiveDirectory.Interfaces
{
    public interface IADContactSearcher
    {
        List<IADContact> FindChangedContacts(bool ignoreDisabledUsers = true, int daysBackToSearch = 90);
        Task<List<IADContact>> FindChangedContactsAsync(bool ignoreDisabledUsers = true);
        IADContact? FindContactsByContainerName(string? searchTerm, bool exactMatch = false);
        IADContact? FindContactsByGUID(byte[]? guid);
        List<IADContact> FindContactsByString(string? searchTerm, bool exactMatch = false);
        Task<List<IADContact>> FindContactsByStringAsync(string? searchTerm, bool exactMatch = false);
        List<IADContact> FindExpiredContacts();
        List<IADContact> FindNewContacts(int maxAgeInDays = 14, bool ignoreDisabledUsers = true);
        Task<List<IADContact>> FindNewContactsAsync(int maxAgeInDays = 14, bool ignoreDisabledUsers = true);
    }
}