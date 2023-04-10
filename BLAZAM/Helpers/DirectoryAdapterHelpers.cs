using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using MudBlazor;

namespace BLAZAM.Server.Helpers
{
    public  static class DirectoryAdapterHelpers
    {
        public static string TypeIcon(this IDirectoryEntryAdapter adapter)
        {
            switch (adapter.ObjectType)
            {
                case Common.Data.ActiveDirectory.ActiveDirectoryObjectType.User:
                    return Icons.Material.Filled.Person;
                case Common.Data.ActiveDirectory.ActiveDirectoryObjectType.Group:
                    return Icons.Material.Filled.Group;
                case Common.Data.ActiveDirectory.ActiveDirectoryObjectType.Computer:
                    return Icons.Material.Filled.Computer;
                case Common.Data.ActiveDirectory.ActiveDirectoryObjectType.OU:
                    return Icons.Material.Filled.Folder;
                default:
                    return Icons.Material.Filled.QuestionMark;
            }
        }
    }
}
