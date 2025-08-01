namespace BLAZAM.Common.Exceptions
{
    [Serializable]
    public class UnresolvableAddressException : AppException
    {
        // Property to hold the specific data point
        public string Address { get; }

        /// <summary>
        /// Creates an exception for an unresolvable address.
        /// </summary>
        /// <param name="address">The address that could not be resolved.</param>
        public UnresolvableAddressException(string address)
             : base(string.Format(Localization.AppExceptionLocalization.UnresolvableAddress, address))
        {
            Address = address;
        }

        /// <summary>
        /// Creates an exception for an unresolvable address with an inner exception.
        /// </summary>
        /// <param name="address">The address that could not be resolved.</param>
        /// <param name="innerException">The inner exception.</param>
        public UnresolvableAddressException(string address, Exception innerException)
            : base(string.Format(Localization.AppExceptionLocalization.UnresolvableAddress, address), innerException)
        {
            Address = address;
        }

       
    }
}