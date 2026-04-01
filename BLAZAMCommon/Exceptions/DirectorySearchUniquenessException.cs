namespace BLAZAM.Pages.API.v1
{
    public class DirectorySearchUniquenessException : AppException
    {
        public string SearchTerm;
        public DirectorySearchUniquenessException(string searchTerm)
        {
            SearchTerm = searchTerm;
        }

        public DirectorySearchUniquenessException(string searchTerm, string? message) : base(message)
        {
            SearchTerm = searchTerm;
        }

        public DirectorySearchUniquenessException(string searchTerm, string? message, Exception? innerException) : base(message, innerException)
        {
            SearchTerm = searchTerm;
        }


    }
}