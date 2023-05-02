using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using MudBlazor;

namespace BLAZAM.Helpers
{
    public  static class DirectoryAdapterHelpers
    {
        public static string TypeIcon(this IDirectoryEntryAdapter adapter)
        {
            switch (adapter.ObjectType)
            {
                case ActiveDirectoryObjectType.User:
                    return Icons.Material.Filled.Person;
                case ActiveDirectoryObjectType.Group:
                    return Icons.Material.Filled.Group;
                case ActiveDirectoryObjectType.Computer:
                    return Icons.Material.Filled.Computer;
                case ActiveDirectoryObjectType.OU:
                    return Icons.Material.Filled.Folder;
                case ActiveDirectoryObjectType.Printer:
                    return Icons.Material.Filled.Print;
                default:
                    return Icons.Material.Filled.QuestionMark;
            }
        }
    }
}
