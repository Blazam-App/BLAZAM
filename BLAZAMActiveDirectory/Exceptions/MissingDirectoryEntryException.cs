namespace BLAZAM.ActiveDirectory.Exceptions
{
    [Serializable]
    public class MissingDirectoryEntryException : ApplicationException
    {
        public MissingDirectoryEntryException()
        {
        }

        public MissingDirectoryEntryException(string? message) : base(message)
        {
        }

        public MissingDirectoryEntryException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}