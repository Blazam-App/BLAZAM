
using System.Net.NetworkInformation;
using System.Net;
using System.Management;
using BLAZAM.Common.Data.Services;
using BLAZAM.Logger;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Models;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class ADComputer : GroupableDirectoryAdapter, IADComputer
    {

        private ADComputerSessions sessionManager;
        private WmiConnection? wmiConnection
        {
            get
            {
                if (CanonicalName == null) return null;
                return new WmiConnection(Directory.Computers.WmiFactory.CreateWmiConnection(CanonicalName));
            }
        }
        private CancellationTokenSource cts;
        private bool? online;
        public ADComputer()
        {
            MonitorOnlineStatus();
        }

        public string? OperatingSystem
        {
            get
            {
                return GetStringProperty(ActiveDirectoryFields.OperatingSystem.FieldName);
            }
            set
            {
                SetProperty(ActiveDirectoryFields.OperatingSystem.FieldName, value);
            }

        }
        public IPHostEntry? IPHostEntry { get; set; }

        /// <summary>
        /// The online status of this computer right now.
        /// Null when still checking, otherwise True or False
        /// </summary>
        public virtual bool? Online
        {
            get => online; protected set
            {

                if (value == online) return;
                online = value;
                if (value != null)
                    OnOnlineChanged?.Invoke((bool)value);
            }
        }

        public List<IADComputerDrive> GetDrives()
        {
            if (wmiConnection == null) return new();
            return wmiConnection.Drives;
        }
        public async Task<List<IADComputerDrive>> GetDrivesAsync()
        {
            if (wmiConnection == null) return new();

            return await Task.Run(() =>
            {
                return wmiConnection.Drives;
            });
        }
        public async Task<List<IRemoteSession>> GetRemoteSessionsAsync()
        {
            if (CanonicalName == null) return new List<IRemoteSession>();
            return await Task.Run(() =>
            {
                if (sessionManager == null)
                {
                    sessionManager = new ADComputerSessions(this);
                }
                return sessionManager.ConnectedSessions;
            });
        }
        public AppEvent<bool> OnOnlineChanged { get; set; }


        /// <summary>
        /// Rename this computer
        /// </summary>
        /// <param name="newName">The new PC name</param>
        internal new bool Rename(string newName)
        {
            try
            {
                // Run.Process("netdom", "renamecomputer " + Name + " /newname:" + newName + " /userd:" + System.Security.Principal.WindowsIdentity.GetCurrent().Name + " /passwordd:* /securepasswordprompt /force", true);
                return true;

            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectryLogger.Error(ex.Message);
            }
            return false;

        }




        protected async void MonitorOnlineStatus(int timeout = 500)
        {
            cts = new CancellationTokenSource();
            await Task.Run(() =>
            {
                if (SearchResult != null && !cts.IsCancellationRequested && CanonicalName != null)
                {
                    try
                    {
                        if (IPHostEntry == null && !cts.IsCancellationRequested)
                        {
                            IPHostEntry = Dns.GetHostEntry(CanonicalName);
                            Task.Delay(60000).ContinueWith((s) =>
                            {
                                IPHostEntry = null;
                            });
                        }
                        Ping ping = new Ping();
                        int retries = 5;
                        int x = 0;
                        do
                        {
                            try
                            {
                                if (!cts.IsCancellationRequested)
                                {
                                    PingReply response = ping.Send(CanonicalName, timeout);
                                    if (response != null)
                                    {
                                        if (response.Status == IPStatus.Success)
                                        {
                                            Online = true;
                                            return;
                                        }
                                        else if (response.Status == IPStatus.TimedOut)
                                        {
                                            Online = false;
                                            return;

                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Loggers.ActiveDirectryLogger.Error(ex.Message);

                                //MainWindow.Get.Toast("Error pinging " + destination);
                                //Debug.WriteLine("Error pinging " + destination);
                            }
                            x++;
                        } while (x < retries);

                    }
                    catch (Exception ex)
                    {
                        Loggers.ActiveDirectryLogger.Error(ex.Message);

                    }
                }

                Online = false;

            }, cts.Token);
            await Task.Delay(1000);
            MonitorOnlineStatus();

        }

        public override void Dispose()
        {
            base.Dispose();
            cts.Cancel();
        }
    }
}
