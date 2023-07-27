using System.IO.Compression;
using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common;
using BLAZAM.Common.Data;
using BLAZAM.Common.Exceptions;
using BLAZAM.Update.Exceptions;
using BLAZAM.FileSystem;
using BLAZAM.Logger;
using BLAZAM.Database.Context;
using BLAZAM.Helpers;

namespace BLAZAM.Update
{
    public enum UpdateStage { None, Downloading, Downloaded, Staging, Staged, BackingUp, Prepared, Applying, Applied };

    public class ApplicationUpdate
    {



        public UpdateStage UpdateStage { get; set; }
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

        public string Branch { get => Release.Branch; }

        private IAppDatabaseFactory _dbFactory;

        /// <summary>
        /// The application update directory, in temporary files
        /// </summary>
        /// <returns>
        /// eg: C:\user\appdata\local\temp\BLAZAM\update\
        ///</returns>
        private static SystemDirectory UpdateTempDirectory { get; set; }

        public static SystemDirectory StagingDirectory =>
            new SystemDirectory(UpdateTempDirectory + "staged\\");

        /// <summary>
        /// The local staging directory path for this update
        /// </summary>
        public SystemDirectory UpdateStagingDirectory { get => new SystemDirectory(StagingDirectory + Version.Version); }



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
            get => new SystemDirectory(UpdateTempDirectory + "backup\\" + _runningVersion + "\\");
        }
        public SystemDirectory BackupDirectory
        {
            get => new SystemDirectory(UpdateTempDirectory + "backup\\" + _runningVersion + "\\");
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
                var testPath = new SystemFile(_applicationRootDirectory + @"bin\Debug\net6.0\updater\update.ps1");


                if (!testPath.Exists)
                {
                    testPath = new SystemFile(_applicationRootDirectory + @"updater\update.ps1");
                }
                return testPath;
            }
        }
        private string CommandArguments
        {
            get
            {
                return " -UpdateSourcePath '" + UpdateStagingDirectory + "' -ProcessId " + _runningProcess.Id + " -ApplicationDirectory '" + _applicationRootDirectory + "'" +
                   " -Username " + DatabaseCache.ActiveDirectorySettings?.Username +
                   " -Domain " + DatabaseCache.ActiveDirectorySettings?.FQDN +
                   " -Password '" + DatabaseCache.ActiveDirectorySettings?.Password.Decrypt() + "'";
            }
        }


        public AppEvent<FileProgress?> DownloadPercentageChanged { get; set; }

        bool _downloaded = false;
        bool _staged = false;
        bool _backedUp = false;
        ApplicationVersion _runningVersion;
        Process _runningProcess;
        SystemDirectory _applicationRootDirectory;

        public ApplicationUpdate(ApplicationInfo applicationInfo, IAppDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;

            UpdateTempDirectory = new SystemDirectory(applicationInfo.TempDirectory + "update\\");
            _runningProcess = applicationInfo.RunningProcess;
            _runningVersion = applicationInfo.RunningVersion;
            _applicationRootDirectory = applicationInfo.ApplicationRoot;
        }


        public bool Newer
        {
            get { return Version.CompareTo(_runningVersion) > 0; }
        }

        public IApplicationRelease Release { get; set; }

        public async Task<bool> Prepare()
        {
            try
            {
                //Confirm the update is downloaded and staged before applying
                if (!_downloaded)
                    await Download();
                if (!_staged)
                    await Stage();

                if (!_backedUp)
                    await Backup();
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
            if (cancellationTokenSource == null || cancellationTokenSource.IsCancellationRequested)
                cancellationTokenSource = new CancellationTokenSource();

            OnUpdateStarted?.Invoke();
            if (!await Prepare())
                throw new ApplicationUpdateException("Update preparation not completed");

            if (cancellationTokenSource.IsCancellationRequested) throw new ApplicationUpdateCanceledException("Update cancelled by user");
            //All prerequisites met


            //Update the updater first
            Loggers.UpdateLogger.Debug("Copying updater script");
            Loggers.UpdateLogger.Debug("Source: " + UpdateStagingDirectory + "\\updater\\*");
            Loggers.UpdateLogger.Debug("Dest: " + _applicationRootDirectory + "updater\\");
            Loggers.UpdateLogger.Debug("Update command: " + UpdateCommand);


            using var context = await _dbFactory.CreateDbContextAsync();

            if (_applicationRootDirectory.Writable)
            {
                Loggers.UpdateLogger.Warning("The application user has write permission to the application directory!");
                try
                {
                    return StartUpdate();
                }
                catch (Exception ex)
                {
                    Loggers.UpdateLogger.Error("Error applying updated updater: {Message}{NewLine}{StackTrace}", ex);

                }
                return "Error starting update";
            }
            else
            {
                var settings = context.ActiveDirectorySettings.FirstOrDefault();


                if (settings == null) throw new ApplicationUpdateException("No credentials are configured for updates");

                var impersonation = settings.CreateWindowsImpersonator();
                try
                {

                    return impersonation.Run(() =>
                    {
                        try
                        {
                            return StartUpdate();
                        }
                        catch (Exception ex)
                        {
                            Loggers.UpdateLogger.Error("Error applying updated updater: {Message}{NewLine}{StackTrace}", ex);

                        }
                        return "Error starting update";
                    });


                }
                catch (ApplicationException ex)
                {
                    return ex.Message;
                }



            }






            throw new ApplicationUpdateException("An unknown error caused the update to fail.");

        }

        private string StartUpdate()
        {
            Loggers.UpdateLogger.Information("Updating updater");

            File.Copy(UpdateStagingDirectory + "\\updater\\*", _applicationRootDirectory + "updater\\", true);
            Loggers.UpdateLogger.Information("Updater updated");
            //If the updater upated we can  run the updater
            var updaterRan = InvokeUpdateExecutable();

            if (updaterRan)
            {
                Loggers.UpdateLogger.Information("Update process started");

                return "Success";
            }
            else
            {

                return "Couldn't start update process!";
            }
        }

        private bool InvokeUpdateExecutable()
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
            try
            {
                var result = await Task.Run(() => { return _applicationRootDirectory.CopyTo(BackupDirectory); });

                Loggers.UpdateLogger.Debug("Backup result: " + result.ToString());

                return result;
            }
            catch (Exception ex)
            {
                Loggers.UpdateLogger.Error("Backup of current version failed: " + ex.Message);
                return false;
            }
        }
        /*
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
        */
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
                        _staged = true;
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
            cancellationTokenSource?.Cancel();
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

            var progress = new FileProgress();
            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(Release.DownloadURL, HttpCompletionOption.ResponseHeadersRead))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        Loggers.UpdateLogger?.Debug("Unable to connect to download url: " + response.StatusCode + " : " + response.ReasonPhrase);

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
                                if (cancellationTokenSource?.IsCancellationRequested != true)
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

                            _downloaded = true;


                        }
                    }
                }
            }





            return true;
        }
    }
}
