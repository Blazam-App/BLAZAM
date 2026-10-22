using System.Text.Json.Serialization;

namespace BLAZAM.Pages.API.Data
{
    /// <summary>
    /// Request payload for updating user details.
    /// </summary>
    public class UpdateUserPayload
    {
        /// <summary>
        /// The given name (first name) of the user.
        /// </summary>
        public string? GivenName { get; set; }

        /// <summary>
        /// The surname (last name) of the user.
        /// </summary>
        public string? Surname { get; set; }

        /// <summary>
        /// The middle name of the user.
        /// </summary>
        public string? MiddleName { get; set; }

        /// <summary>
        /// The primary email address of the user.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// The description of the user account.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Additional custom attributes/fields to set.
        /// </summary>
        public List<NewUserField>? Fields { get; set; }
    }
}
