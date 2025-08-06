using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.FileSystem;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.JSInterop;

namespace BLAZAM.Gui.UI.Computers
{
    public partial class ComputerFileSystemBrowser : ComputerViewBase
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; }
        private IEnumerable<FileSystemBase> Items { get; set; } = new List<FileSystemBase>();
        private SystemDirectory? CurrentPath { get; set; }

        protected override void OnParametersSet()
        {
            if (Computer != null && CurrentPath == null)
            {
                UpdateDrives();
            }
        }

        private void UpdateDrives()
        {
            if (Computer == null) return;
            Items = Computer.Directory.Impersonation.Run(() =>
            {
                return Computer.GetDrives().Select(d => new SystemDirectory(d.Name + "\\"));
            });
            StateHasChanged();
        }

        private void RowClicked(FileSystemBase item)
        {
            if (item is SystemDirectory dir)
            {
                CurrentPath = dir;
                UpdateFileList();
            }
        }

        private void GoUp()
        {
            if (CurrentPath?.ParentDirectory != null)
            {
                CurrentPath = CurrentPath.ParentDirectory;
                UpdateFileList();
            }
            else
            {
                CurrentPath = null;
                UpdateDrives();
            }
        }

        private void UpdateFileList()
        {
            if (Computer == null || CurrentPath == null)
            {
                UpdateDrives();
                return;
            }
            Items = Computer.Directory.Impersonation.Run(() =>
            {
                var items = new List<FileSystemBase>();
                items.AddRange(CurrentPath.SubDirectories);
                items.AddRange(CurrentPath.Files);
                return items.OrderBy(i => i is SystemDirectory ? 0 : 1).ThenBy(i => i.Name);
            });
            StateHasChanged();
        }

        private async void Rename(FileSystemBase item)
        {
            var parameters = new DialogParameters { ["Item"] = item };
            var dialog = DialogService.Show<BLAZAM.Gui.UI.Modals.RenameFileSystemItemModal>("Rename", parameters);
            var result = await dialog.Result;
            if (!result.Cancelled)
            {
                var newName = result.Data.ToString();
                if (Computer.Directory.Impersonation.Run(() => item.Rename(newName)))
                {
                    UpdateFileList();
                }
                else
                {
                    Snackbar.Add("Failed to rename item.", Severity.Error);
                }
            }
        }
        private async void Delete(FileSystemBase item)
        {
            var result = await DialogService.ShowMessageBox(
                "Delete",
                $"Are you sure you want to delete {item.Name}?",
                yesText: "Delete", cancelText: "Cancel");

            if (result == true)
            {
                if (Computer.Directory.Impersonation.Run(() =>
                {
                    if (item is SystemFile file)
                    {
                        file.Delete();
                        return !file.Exists;
                    }
                    else if (item is SystemDirectory dir)
                    {
                        dir.Delete(true);
                        return !dir.Exists;
                    }
                    return false;
                }))
                {
                    UpdateFileList();
                }
                else
                {
                    Snackbar.Add("Failed to delete item.", Severity.Error);
                }
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
                Snackbar.Add("Failed to download file.", Severity.Error);
            }
        }
    }
}
