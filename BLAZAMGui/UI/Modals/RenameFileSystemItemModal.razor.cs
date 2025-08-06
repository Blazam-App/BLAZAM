using BLAZAM.FileSystem;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BLAZAM.Gui.UI.Modals
{
    public partial class RenameFileSystemItemModal
    {
        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public FileSystemBase Item { get; set; }

        private string NewName { get; set; }

        protected override void OnInitialized()
        {
            if(Item != null)
                NewName = Item.Name;
        }

        private void Submit()
        {
            MudDialog.Close(DialogResult.Ok(NewName));
        }

        void Cancel() => MudDialog.Cancel();
    }
}
