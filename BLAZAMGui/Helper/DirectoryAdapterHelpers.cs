using MudBlazor;

namespace BLAZAM.Helpers
{
    public static class DirectoryAdapterHelpers
    {
        /// <summary>
        /// Retuns a MudBlazor Icon that represents this <see cref="IDirectoryEntryAdapter"/>
        /// </summary>
        /// <param name="adapter"></param>
        /// <returns></returns>
        public static string TypeIcon(this IDirectoryEntryAdapter adapter)
        {
            return adapter.ObjectType.TypeIcon();
        }
        /// <summary>
        /// Retuns a MudBlazor Icon that represents this <see cref="ActiveDirectoryObjectType"/>
        /// </summary>
        /// <param name="adapter"></param>
        /// <returns></returns>
        public static string TypeIcon(this ActiveDirectoryObjectType adapter)
        {
            switch (adapter)
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
                case ActiveDirectoryObjectType.Contact:
                    return Icons.Material.Filled.Contacts;
                case ActiveDirectoryObjectType.BitLocker:
                    return Icons.Material.Filled.EnhancedEncryption;
                case ActiveDirectoryObjectType.All:
                    return Icons.Material.Filled.AccountTree;
                default:
                    return Icons.Material.Filled.QuestionMark;
            }
        }
    }
}
