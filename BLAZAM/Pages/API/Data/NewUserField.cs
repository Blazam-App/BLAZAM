using System.Text.Json;

namespace BLAZAM.Pages.API.Data
{
    /// <summary>
    /// Represents an Active Directory attribute field to set for thee new user
    /// </summary>
    public class NewUserField
    {
        /// <summary>
        /// The attribute name as set in Active Directory
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// The value to set for this attribute field
        /// </summary>
        public object? FieldValue { get; set; }
    }
}