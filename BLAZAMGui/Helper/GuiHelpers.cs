using ApplicationNews;
using BLAZAM.Database.Models.Templates;
using BLAZAM.Gui.UI.Modals;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Helpers
{
    public static class GuiHelpers
    {
            public static async Task<byte[]?> ToByteArrayAsync(this IBrowserFile file, int maxReadBytes = 5000000)
            {
                byte[] fileBytes;
                using (var stream = file.OpenReadStream(5000000))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        fileBytes = memoryStream.ToArray();
                    }
                }
                return fileBytes;
            }

        public static async Task<IDialogReference> ShowNewsItemDialog(this NewsItem item,AppDialogService dialogService)
        {
            var dialogParams = new DialogParameters
            {
                { "Item", item }
            };
            var options = new DialogOptions();
            options.MaxWidth = MaxWidth.ExtraExtraLarge;
            options.CloseButton = true;
            options.CloseOnEscapeKey = true;
            
            return (await dialogService.ShowMessage<AppNewsItemDialog>(dialogParams, item.Title,options:options));

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
