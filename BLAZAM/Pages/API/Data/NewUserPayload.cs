namespace BLAZAM.Pages.API.Data
{
    /// <summary>
    /// Request package for creation of a templated user
    /// </summary>
    public class NewUserPayload
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
        /// Overrides the OU in a template if provided.
        /// </remarks>
        public string? OU { get; set; }

        /// <summary>
        /// The fields to set for this user. Template field with values will also
        /// be applied.
        /// </summary>
        public List<NewUserField>? Fields { get; set; } = new();
        /// <summary>
        /// A list of groups to assign for this user. The value can be the SID, DN, or
        /// group name. If using group name, the name must be unique and match a single
        /// group in the domain. Template groups will also be applied.
        /// </summary>
        public List<string>? Groups { get; set; } = new();

        /// <summary>
        /// If the template is set to send a welcome email, and requests a destination, will be sent
        /// to this email address.
        /// </summary>
        public string? SendWelcomeEmailTo { get; set; }
    }
    /// <summary>
    /// Example details for Swagger documentation
    /// </summary>
    public class NewUserDetailsExample
    {
        /// <summary>
        /// Returns an example NewUserDetails object for Swagger documentation
        /// </summary>
        /// <returns></returns>
        public object GetExamples()
        {
            return new NewUserPayload
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                OU = "OU=Users,DC=example,DC=com",
                Fields = new List<NewUserField>
            {
                new() { FieldName = "Department", FieldValue = "Sales" },
                new() { FieldName = "Title", FieldValue = "Sales Representative" }
            },
                Groups = new List<string> { "S-1-5-21-1004336348-1177238915-682003330-512", "S-1-5-21-1004336348-1148567915-615476330-495" }
            };
        }
    }
}
