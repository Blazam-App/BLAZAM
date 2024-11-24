using BLAZAM.Database.Models.Templates;
using System.Text.Json;

namespace BLAZAM.Pages.API.Data
{
    
    public class NewUserDetails
    {
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
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


        public List<NewUserField>? Fields { get; set; }
        public List<string>? Groups { get; set; }
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
