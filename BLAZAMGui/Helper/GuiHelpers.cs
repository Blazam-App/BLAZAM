
using ApplicationNews;
using BLAZAM.ActiveDirectory.Data;
using BLAZAM.Gui.UI.Modals;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace BLAZAM.Helpers
{
    public static class GuiHelpers
    {
        public static async Task<byte[]?> ReadByteArrayAsync(this IBrowserFile file, int maxReadBytes = 5000000)
        {
            byte[] fileBytes;
            using (var stream = file.OpenReadStream(maxReadBytes))
            {
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }
            }
            return fileBytes;
        }
        public static ActiveDirectoryUserState ToActiveDirectoryUserState(this IApplicationUserState userState)
        {
            return new ActiveDirectoryUserState()
            {
                Username = userState.AuditUsername,
                PermissionMappings = userState.PermissionMappings,
                IsSuperAdmin = userState.IsSuperAdmin

            };

        }
        public static async Task<IDialogReference> ShowNewsItemDialog(this NewsItem item, AppDialogService dialogService)
        {
            var dialogParams = new DialogParameters
            {
                { "Item", item }
            };
            var options = new DialogOptions()
            {
                MaxWidth = MaxWidth.ExtraExtraLarge,
                CloseButton = true,
                CloseOnEscapeKey = true

            };


            return (await dialogService.ShowMessage<AppNewsItemDialog>(dialogParams, item.Title, options: options));

        }

        public static List<TreeItemData<IDirectoryEntryAdapter>> ToTreeItemData(this IEnumerable<IDirectoryEntryAdapter> items)
        {
            List<TreeItemData<IDirectoryEntryAdapter>> treeData = new();

            items.ForEach(x =>
            {
                treeData.Add(new() { Value = x, Selected = x.IsSelected });

            });
            return treeData;
        }

        public static List<TreeItemData<IADOrganizationalUnit>> ToTreeItemData(this IEnumerable<IADOrganizationalUnit> items)
        {
            List<TreeItemData<IADOrganizationalUnit>> treeData = new();

            items.ForEach(x =>
            {
                treeData.Add(new() { Value = x, Selected = x.IsSelected });

            });
            return treeData;
        }
        public static List<TreeItemData<T>> ToTreeItemData<T>(this IEnumerable<T> items)
        {
            List<TreeItemData<T>> treeData = new();

            items.ForEach(x =>
            {
                treeData.Add(new() { Value = x });

            });
            return treeData;
        }

    }
}
