using BLAZAM.Common.Data.ActiveDirectory;

namespace BLAZAM.Common.Models.Database
{
    public interface IActiveDirectoryField
    {
        string DisplayName { get; set; }
        string FieldName { get; set; }
        ActiveDirectoryFieldType FieldType { get; set; }

        bool Equals(object? obj);
        int GetHashCode();
        bool IsActionAppropriateForObject(ActiveDirectoryObjectType objectType);
        string? ToString();
    }
}