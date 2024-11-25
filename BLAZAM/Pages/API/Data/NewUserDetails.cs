using BLAZAM.Database.Models.Templates;
using System.Text.Json;

namespace BLAZAM.Pages.API.Data
{
    /// <summary>
    /// Request package for creation of a templated user
    /// </summary>
    public class NewUserDetails
    {
        /// <summary>
        /// The given name for this user
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// The middle name for this user
        /// </summary>
        public string? MiddleName { get; set; }
        /// <summary>
        /// The surname for this user
        /// </summary>
        public string? LastName { get; set; }
        /// <summary>
        /// If set, overrides the template generated username
        /// </summary>
        public string? Username { get; set; }
        /// <summary>
        /// The Distinguished Name of the Organizational Unit to 
        /// create the new user under
        /// </summary>
        /// <remarks>
        /// Only used for custom user creation, ignored for API template execution
        /// </remarks>
        public string? OU { get; set; }

        /// <summary>
        /// The fields to set for this user. Template field with values will also
        /// be applied.
        /// </summary>
        public List<NewUserField>? Fields { get; set; }
        /// <summary>
        /// A list of group SID's to assign for this user. Template groups will also
        /// be applied.
        /// </summary>
        public List<string>? Groups { get; set; }

        /// <summary>
        /// If the template is set to send a welcome email, and requests a destination, will be sent
        /// to this email address.
        /// </summary>
        public string? SendWelcomeEmailTo { get; set; }
    }
    public class NewUserDetailsExample
    {
        public object GetExamples()
        {
            return new NewUserDetails
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                OU = "OU=Users,DC=example,DC=com",
                Fields = new List<NewUserField>
            {
                new NewUserField { FieldName = "Department", FieldValue = "Sales" },
                new NewUserField { FieldName = "Title", FieldValue = "Sales Representative" }
            },
                Groups = new List<string> { "S-1-5-21-1004336348-1177238915-682003330-512", "S-1-5-21-1004336348-1148567915-615476330-495" }
            };
        }
    }
}
