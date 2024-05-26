using BLAZAM.Common.Data;

namespace BLAZAM.Database.Models
{
    public interface IActiveDirectoryField
    {
        /// <summary>
        /// The display name for this field in the application
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// The attribute name in Active Directory
        /// </summary>
        string FieldName { get; set; }

        /// <summary>
        /// The data type for this field
        /// </summary>
        ActiveDirectoryFieldType FieldType { get; set; }

        bool Equals(object? obj);

        /// <summary>
        /// Computes a hash based on the <see cref="FieldName"/>
        /// </summary>
        /// <returns>The hash code of the <see cref="FieldName"/></returns>
        int GetHashCode();

        /// <summary>
        /// Indicates whether this field applies to the requested <see cref="ActiveDirectoryObjectType"/>
        /// </summary>
        /// <param name="objectType">The Active Directory object type</param>
        /// <returns>True if this field should apply, otherwise false</returns>
        bool IsFieldAppropriateForObject(ActiveDirectoryObjectType objectType);

        /// <summary>
        /// 
        /// </summary>
        /// <returns><see cref="DisplayName"/></returns>
        string? ToString();
    }
}