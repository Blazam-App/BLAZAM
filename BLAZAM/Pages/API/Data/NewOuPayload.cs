namespace BLAZAM.Pages.API.Data
{
    /// <summary>
    /// Request payload for creating a new Organizational Unit.
    /// </summary>
    public class NewOuPayload
    {
        /// <summary>
        /// The name of the new Organizational Unit.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Distinguished Name (DN) of the parent Organizational Unit.
        /// </summary>
        public string ParentOU { get; set; }

        /// <summary>
        /// Description of the Organizational Unit.
        /// </summary>
        public string? Description { get; set; }
    }
}
