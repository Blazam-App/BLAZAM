namespace BLAZAM.ActiveDirectory.Interfaces
{
    /// <summary>
    /// Represents an Active Directory contact object, providing access to contact-specific properties.
    /// </summary>
    /// <remarks>
    /// This interface extends <see cref="IGroupableDirectoryAdapter"/> to include properties commonly associated with AD contacts,
    /// such as address, company, and personal details.
    /// </remarks>
    public interface IADContact : IGroupableDirectoryAdapter
    {
        /// <summary>
        /// Gets or sets the city associated with the contact's address.
        /// </summary>
        string? City { get; set; }

        /// <summary>
        /// Gets or sets the company name for the contact.
        /// </summary>
        string? Company { get; set; }

        /// <summary>
        /// Gets or sets the department within the company for the contact.
        /// </summary>
        string? Department { get; set; }

        /// <summary>
        /// Gets or sets the employee ID for the contact, if applicable.
        /// </summary>
        string? EmployeeId { get; set; }

        /// <summary>
        /// Gets or sets the given (first) name of the contact.
        /// </summary>
        string? GivenName { get; set; }

        /// <summary>
        /// Gets or sets the home phone number of the contact.
        /// </summary>
        string? HomePhone { get; set; }

        /// <summary>
        /// Gets or sets the middle name of the contact.
        /// </summary>
        string? MiddleName { get; set; }

        /// <summary>
        /// Gets or sets the office name or location for the contact.
        /// </summary>
        string? PhysicalDeliveryOfficeName { get; set; }

        /// <summary>
        /// Gets or sets the site or location associated with the contact.
        /// </summary>
        string? Site { get; set; }

        /// <summary>
        /// Gets or sets the state or province for the contact's address.
        /// </summary>
        string? State { get; set; }

        /// <summary>
        /// Gets or sets the post office box number for the contact's address.
        /// </summary>
        string? POBox { get; set; }

        /// <summary>
        /// Gets or sets the street address for the contact.
        /// </summary>
        string? StreetAddress { get; set; }

        /// <summary>
        /// Gets or sets the primary telephone number for the contact.
        /// </summary>
        string? TelephoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the job title of the contact.
        /// </summary>
        string? Title { get; set; }

        /// <summary>
        /// Gets or sets the ZIP or postal code for the contact's address.
        /// </summary>
        string? Zip { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail photo of the contact, if available.
        /// </summary>
        byte[]? ThumbnailPhoto { get; set; }

        /// <summary>
        /// Gets or sets the surname (last name) of the contact.
        /// </summary>
        string? Sn { get; set; }
    }
}