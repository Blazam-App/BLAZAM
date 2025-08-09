using BLAZAM.FileSystem;
using MudBlazor;

namespace BLAZAM.Gui.UI.Computers
{
    public partial class ComputerFileSystemBrowser : ComputerViewBase
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; }
        private MudDataGrid<FileSystemBase>? _dataGrid;
        private IEnumerable<FileSystemBase> items { get; set; } = new List<FileSystemBase>();
        private SystemDirectory? currentPath { get; set; }
        private List<IADComputerDrive> _drives = [];
        private IADComputer? _computer;
        protected override void OnParametersSet()
        {
            if (Computer != null && !Computer.Equals(_computer))
            {

                _computer = Computer;
                _drives = _computer.GetDrives();
                if (_drives.Count > 0)
                {
                    currentPath = _drives[0].UNCPath;
                    LoadItems();
                }
            }
        }

        private async void LoadItems()
        {
            if (currentPath != null)
            {
                await _computer.Directory.Impersonation.RunAsync(() =>
                {
                    items = currentPath.Files;
                    return true;
                });
                await _computer.Directory.Impersonation.RunAsync(() =>
                {
                    var directories = currentPath.SubDirectories;
                    directories.ForEach(dir =>
                    {
                        items = items.Append(dir);
                    });
                    InvokeAsync(StateHasChanged);
                    return true;
                });
            }
        }




        private async void Download(SystemFile item)
        {
            var fileBytes = await Computer.Directory.Impersonation.Run(async () => await item.ReadAllBytesAsync());
            if (fileBytes != null && fileBytes.Length > 0)
            {
                await JSRuntime.InvokeVoidAsync("downloadFile", item.Name, Convert.ToBase64String(fileBytes));
            }
            else
            {
                SnackBarService.Error("Failed to download file.");
            }
        }
    }
}
