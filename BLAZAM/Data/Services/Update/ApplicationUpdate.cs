using Microsoft.TeamFoundation.Build.WebApi;
using System.IO.Compression;
using BLAZAM.Common.Data.ActiveDirectory.Models;
using Blazorise;
using Microsoft.VisualStudio.Services.Common.CommandLine;
using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.Build.Framework;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common;
using BLAZAM.Common.Data.FileSystem;
using BLAZAM.Common.Data;

namespace BLAZAM.Server.Data.Services.Update
{
    public enum UpdateStage { None, Downloading, Downloaded, Staging, Staged, BackingUp, Prepared, Applying, Applied };

    public class ApplicationUpdate
    {




        /// <summary>
        /// Token source for cancelling this update when in progress
        /// </summary>
        private CancellationTokenSource cancellationTokenSource { get; set; }

        public static AppEvent OnUpdateStarted { get; set; }
        public static AppEvent<Exception> OnUpdateFailed { get; set; }

        /// <summary>
        /// The version of this update
        /// </summary>
        public ApplicationVersion Version { get => Release.Version; }


        /// <summary>
        /// The application update directory, in temporary files
        /// </summary>
        /// <returns>
        /// eg: C:\user\appdata\local\temp\BLAZAM\update\
        ///</returns>
        private static SystemDirectory UpdateTempDirectory
            => new SystemDirectory(Program.TempDirectory + "update\\");

        public static SystemDirectory StagingDirectory =>
            new SystemDirectory(UpdateTempDirectory + "staged\\");

        /// <summary>
        /// The local path for staging directory path for this update
        /// </summary>
        public SystemDirectory UpdateStagingDirectory { get => new SystemDirectory(StagingDirectory + Version.Version); }

        /// <summary>
        /// The local path within the staging directory from which
        /// to mirror to the application root path 
        /// </summary>
        /// <returns>
        /// eg: C:\inetpub\blazam\Writable\Update\Staging\0.5.4.2023.1.22.2245\_BLAZAM
        /// </returns>
        public SystemDirectory UpdateSourcePath { get => UpdateStagingDirectory; }

        /// <summary>
        /// The local path to the directory containing the downloaded update zip file
        /// </summary>
        /// <returns>
        /// eg: C:\inetpub\blazam\Writable\Update\Download\
        /// </returns>
        public static SystemDirectory UpdateDownloadDirectory
        {
            get => new SystemDirectory(UpdateTempDirectory + "download\\");
        }
        public SystemDirectory BackupPath
        {
            get => new SystemDirectory(UpdateTempDirectory + "backup\\" + Program.Version + "\\");
        }
        public SystemDirectory BackupDirectory
        {
            get => new SystemDirectory(UpdateTempDirectory + "backup\\" + Program.Version + "\\");
        }

        /// <summary>
        /// The local path to the downloaded zip file
        /// </summary>
        public SystemFile UpdateFile { get => new SystemFile(UpdateDownloadDirectory + Version.Version + ".zip"); }
        public string UpdateCommand => UpdateCommandProcess + " " + UpdateCommandArguments;
        public string UpdateCommandProcess
        {
            get
            {
                return "cmd";
            }
        }
        public string UpdateCommandArguments
        {
            get
            {
                return "/c start Powershell -ExecutionPolicy Bypass -command \"" + CommandProcessPath + CommandArguments + "\"";
            }
        }
        private SystemFile CommandProcessPath
        {
            get
            {
                var testPath = new SystemFile(Program.RootDirectory + @"bin\Debug\net6.0\updater\update.ps1");


                if (!testPath.Exists)
                {
                    testPath = new SystemFile(Program.RootDirectory + @"updater\update.ps1");
                }
                return testPath;
            }
        }
        private string CommandArguments
        {
            get
            {
                return " -UpdateSourcePath '" + UpdateSourcePath + "' -ProcessId " + Program.ApplicationProcess.Id + " -ApplicationDirectory '" + Program.RootDirectory + "'" +
                   " -Username " + DatabaseCache.ActiveDirectorySettings?.Username + " -Domain " + DatabaseCache.ActiveDirectorySettings?.FQDN + " -Password '" + Encryption.Instance.DecryptObject<string>(DatabaseCache.ActiveDirectorySettings?.Password) + "'";
            }
        }


        public AppEvent<FileProgress> DownloadPercentageChanged { get; set; }

        bool downloaded = false;
        bool staged = false;
        bool backedUp = false;

        public bool Newer
        {
            get { return Version.CompareTo(Program.Version) > 0; }
        }

        internal IApplicationRelease Release { get; set; }

        public async Task<bool> Prepare()
        {
            try
            {
                //Confirm the update is downloaded and staged before applying
                if (!downloaded)
                    await Download();
                if (!staged)
                    await Stage();
#if !DEBUG
                if (!backedUp)
                    await Backup();
#endif
                //Confirm staging went as expected
                return UpdateStagingDirectory.Exists;
            }
            catch
            {
                return false;
            }
        }


        public async Task<string> Apply()
        {
            OnUpdateStarted?.Invoke();
            if (!await Prepare())
                throw new ApplicationUpdateException("Update preparation not completed");

            if (cancellationTokenSource.IsCancellationRequested) throw new ApplicationUpdateCanceledException("Update cancelled by user");
            //All prerequisites met


            //Update the updater first
            Loggers.UpdateLogger.Debug("Copying updater script");
            Loggers.UpdateLogger.Debug("Source: " + UpdateSourcePath + "\\updater\\*");
            Loggers.UpdateLogger.Debug("Dest: " + Program.RootDirectory + "updater\\");
            Loggers.UpdateLogger.Debug("Update command: " + UpdateCommand);

            WindowsImpersonation.Run(() =>
            {
                try
                {
                    Loggers.UpdateLogger.Information("Updating updater");

                    File.Copy(UpdateSourcePath + "\\updater\\*", Program.RootDirectory + "updater\\", true);
                    Loggers.UpdateLogger.Information("Updater updated");

                    return true;
                }
                catch (Exception ex)
                {
                    Loggers.UpdateLogger.Error("Error applying updated updater", ex);

                }
                return false;
            });

            //If the updater upated we can  run the updater
            var updaterRan = await InvokeUpdateExecutable();

            if (updaterRan)
            {
                Loggers.UpdateLogger.Information("Update process started");

                return "Success";
            }
            else
            {
                Loggers.UpdateLogger.Information("Attempting backup of current version to: " + BackupPath);

                return "Couldn't start update process!";
            }





            throw new ApplicationUpdateException("An unknown error caused the update to fail.");

        }

        private async Task<bool> InvokeUpdateExecutable()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = UpdateCommandProcess,
                    Arguments = UpdateCommandArguments,
                    //RedirectStandardOutput = true,
                    //UseShellExecute = false,
                    UseShellExecute = true,
                    CreateNoWindow = true,


                }
            };

            process.Start();



            process.WaitForExit();

            return true;
        }

        public async Task<bool> Backup()
        {
            Loggers.UpdateLogger.Information("Attempting backup of current version to: " + BackupPath);

            var result = await Task.Run(() => { return Program.RootDirectory.CopyTo(BackupDirectory); });

            Loggers.UpdateLogger.Debug("Backup result: " + result.ToString());

            return result;
        }

        /// <summary>
        /// Checks if the running application identity has write
        /// permission to a specified path. It does this by creating
        /// a randomly named test file, writing to it, and deleting it.
        /// If all these tests pass the return is true.
        /// </summary>
        /// <param name="rootPath">The directory to test permissions against.</param>
        /// <returns>True if the application identity has write permission to the specified folder, otherwise false.</returns>
        private bool HasWritePermissions(string rootPath)
        {
            var testFilePath = rootPath + "WritePermissionSecurityTest-" + Guid.NewGuid();
            try
            {
                //File.Create(testFilePath);
                File.WriteAllText(testFilePath, "Test Write");
                File.Delete(testFilePath);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> CleanDownload()
        {
            return await Task.Run(() =>
            {
                Loggers.UpdateLogger.Information("Attempting cleaning of download folder: " + UpdateFile);

                try
                {
                    UpdateFile.Delete();
                    return true;

                }
                catch (Exception ex)
                {
                    Loggers.UpdateLogger.Error("Error while cleaning of download folder: " + UpdateFile, ex);

                    return false;
                }
            });
        }
        public async Task<bool> CleanStaging()
        {
            return await Task.Run(() =>
            {
                try
                {
                    UpdateStagingDirectory.Delete(true);
                    return true;

                }
                catch
                {
                    return false;
                }
            });
        }
        public async Task<bool> Stage()
        {
            return await Task.Run(() =>
            {

                if (!UpdateFile.Exists) return false;

                Loggers.UpdateLogger.Debug("Attempting unzip of " + UpdateFile);

                UpdateStagingDirectory.EnsureCreated();

                using (var streamToReadFrom = UpdateFile.OpenReadStream())
                {
                    try
                    {
                        var zip = new ZipArchive(streamToReadFrom);
                        zip.ExtractToDirectory(UpdateStagingDirectory.Path, true);
                        staged = true;
                        Loggers.UpdateLogger.Debug(UpdateFile + " unzipped successfully to " + UpdateStagingDirectory);

                        return true;
                    }
                    catch (Exception ex)
                    {
                        Loggers.UpdateLogger.Error("Error while extracting update zip", ex);

                        return false;
                    }
                }
            });

        }
        public void Cancel()
        {
            cancellationTokenSource.Cancel();
        }
        public async Task<bool> Download()
        {

            if (Release == null)
            {
                return false;
            }
            Loggers.UpdateLogger?.Debug("Attempting download of update " + Version);
            Loggers.UpdateLogger?.Debug("Download URL: " + Release.DownloadURL);
            Loggers.UpdateLogger?.Debug("Download Path: " + UpdateDownloadDirectory);

            cancellationTokenSource = new CancellationTokenSource();
            var progress = new FileProgress();
            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(Release.DownloadURL, HttpCompletionOption.ResponseHeadersRead))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        Loggers.UpdateLogger.Debug("Unable to connect to download url: " + response.StatusCode + " : " + response.ReasonPhrase);

                        return false;
                    }
                    UpdateDownloadDirectory.EnsureCreated();
                    if (UpdateFile.Exists) UpdateFile.Delete();
                    using (var streamToReadFrom = await response.Content.ReadAsStreamAsync())
                    {
                        using (var streamToWriteTo = UpdateFile.OpenWriteStream())
                        {
                            progress.ExpectedSize = (int)Release.ExpectedSize;
                            var buffer = new byte[4096];
                            int bytesRead;
                            int totalBytesRead = 0;

                            while ((bytesRead = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                if (!cancellationTokenSource.IsCancellationRequested)
                                {
                                    await streamToWriteTo.WriteAsync(buffer, 0, bytesRead);
                                    totalBytesRead += bytesRead;
                                    progress.CompletedBytes = totalBytesRead;

                                    DownloadPercentageChanged?.Invoke(progress);
                                }
                                else
                                {
                                    DownloadPercentageChanged?.Invoke(null);

                                    return false;
                                }
                            }

                            downloaded = true;

                            // await streamToReadFrom.CopyToAsync(streamToWriteTo,);
                            // downloaded = true;
                        }
                    }
                }
            }





            return true;
        }
    }
}
