using BLAZAM.Common.Exceptions;

namespace BLAZAM.ActiveDirectory.Exceptions
{
    [Serializable]
    public class MissingDirectoryEntryException : AppException
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