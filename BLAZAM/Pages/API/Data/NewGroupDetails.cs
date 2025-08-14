namespace BLAZAM.Pages.API.Data
{
    /// <summary>
    /// Request package for creation of a group
    /// </summary>
    public class NewGroupDetails
    {
        /// <summary>
        /// The name for this group
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The description for this group
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// The email for this group
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// The Distinguished Name of the Organizational Unit to
        /// create the new group under
        /// </summary>
        public string OU { get; set; }


        /// <summary>
        /// A list of groups to make this new group a member of. The value can be the SID, DN, or
        /// group name. If using group name, the name must be unique and match a single
        /// group in the domain.
        /// </summary>
        public List<string>? Groups { get; set; } = new();

    }
    /// <summary>
    /// Example details for Swagger documentation
    /// </summary>
    public class NewGroupDetailsExample
    {
        /// <summary>
        /// Provides an example of the NewGroupDetails object for Swagger documentation
        /// </summary>
        /// <returns></returns>
        public object GetExamples()
        {
            return new NewGroupDetails
            {
                Name = "MyNewGroup",
                Description = "A new group for testing",
                Email = "test@example.com",
                OU = "OU=Groups,DC=example,DC=com",
                Groups = new List<string> { "S-1-5-21-1004336348-1177238915-682003330-512", "S-1-5-21-1004336348-1148567915-615476330-495" }
            };
        }
    }
}
