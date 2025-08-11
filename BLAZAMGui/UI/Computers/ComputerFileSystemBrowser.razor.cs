using BLAZAM.FileSystem;
using MudBlazor;

namespace BLAZAM.Gui.UI.Computers
{
    public partial class ComputerFileSystemBrowser : ComputerViewBase
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; }
        private MudDataGrid<IFileSystemObject>? _dataGrid;
        private IEnumerable<IFileSystemObject> items { get; set; } = new List<IFileSystemObject>();
        private SystemDirectory? currentPath { get; set; }
        private List<IADComputerDrive> _drives = [];
        private IADComputer? _computer;
        protected override void OnParametersSet()
        {
            if (Computer != null && !Computer.Equals(_computer))
            {

                _computer = Computer;
             
                    LoadDrives(_computer.IsOnline == true);
                
                _computer.OnOnlineChanged += LoadDrives;
            }
        }

        private void LoadDrives(bool online)
        {
            if (online && _computer!=null)
            {
                _drives = _computer.GetDrives();
                if (_drives.Count > 0)
                {
                    currentPath = _drives[0].UNCPath;
                    LoadItems();
                }
            }
        }

        /// <summary>
        /// Loads the items in the current directory.
        /// </summary>
        private async void LoadItems()
        {
            if (currentPath != null && _computer!=null && _computer.IsOnline==true)
            {
                await _computer.Directory.Impersonation.RunAsync(() =>
                {
                    items = currentPath.SubDirectories;
                    var files = currentPath.Files;
                    files.ForEach(file =>
                    {
                        items = items.Append(file);
                    });
                    InvokeAsync(StateHasChanged);
                    return true;
                });
            }
        }
        /// <summary>
        /// Handles the row click event in the data grid.
        /// </summary>
        /// <param name="args"></param>
        private async void RowClicked(DataGridRowClickEventArgs<IFileSystemObject>args)
        {
            var item = args.Item;
            if(item is SystemFile file)
            {
                //TODO: Check if the file is a valid file type for download
            }
            else if (item is SystemDirectory directory)
            {
                currentPath = directory;
                LoadItems();
            }
        }

        private void GoUpOneDirectory()
        {
            if (currentPath?.ParentDirectory != null)
            {
                currentPath = currentPath.ParentDirectory;
                LoadItems();
            }
        }
        private bool CanGoUp => (currentPath == null || currentPath.ParentDirectory == null || !currentPath.ParentDirectory.Exists);

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
