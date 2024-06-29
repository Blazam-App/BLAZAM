
using System.Net.NetworkInformation;
using System.Net;
using BLAZAM.Logger;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Models;
using System.Net.Sockets;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Common.Data;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class ADComputer : AccountDirectoryAdapter, IADComputer
    {

        private ADComputerSessions sessionManager;
        private WmiConnection? wmiConnection
        {
            get
            {
                if (CanonicalName == null) return null;
                return new WmiConnection(Directory.Computers.WmiFactory.CreateWmiConnection(CanonicalName), this);
            }
        }
        private CancellationTokenSource cts;
        private bool? online;
        public ADComputer()
        {

        }

        public async Task<List<IADBitLockerRecovery>?> GetBitLockerRecoveryAsync()
        {
            var recovery = await Directory.BitLocker.FindByComputerAsync(this);
            return recovery;
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
        public virtual bool? IsOnline
        {
            get => online; protected set
            {

                if (value == online) return;
                online = value;
                if (value != null)
                    OnOnlineChanged?.Invoke((bool)value);
            }
        }
        public List<ComputerService> Services => wmiConnection?.Services ?? new();
        public ComputerMemory Memory => wmiConnection?.Memory ?? new();
        public int Processor => wmiConnection?.Processor ?? 0;
        public double MemoryUsedPercent => wmiConnection?.Memory.PercentUsed ?? 0;
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

        public List<SharedPrinter> SharedPrinters
        {
            get
            {
                return wmiConnection?.SharePrinters ?? new();

            }
        }

        public bool CanReadBitLocker
        {
            get
            {
                return HasPermission(p => p.Where(pm =>
                pm.AccessLevels.Any(al =>
                al.ObjectMap.Any(om =>
                om.ObjectType == ActiveDirectoryObjectType.BitLocker &&
                om.ObjectAccessLevel.Level > ObjectAccessLevels.Deny.Level
                ))),
                p => p.Where(pm =>
                pm.AccessLevels.Any(al =>
                al.ObjectMap.Any(om =>
                om.ObjectType == ActiveDirectoryObjectType.BitLocker &&
                om.ObjectAccessLevel.Level == ObjectAccessLevels.Deny.Level
                )))
                );
            }

        }


        /// <summary>
        /// Rename this computer
        /// </summary>
        /// <param name="newName">The new PC name</param>
        internal new bool Rename(string newName)
        {
            try
            {

                // Run.Process("netdom", "renamecomputer " + Name + " /newname:" + newName + " /userd:" + System.Security.Principal.WindowsIdentity.GetCurrent().Name + " /passwordd:* /securepasswordprompt /force", true);
                return false;

            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectryLogger.Error(ex.Message + " {@Error}", ex);
            }
            return false;

        }



        /// <summary>
        /// When called this computers network reachability will be continuously monitored.
        /// </summary>
        /// <param name="timeout"></param>
        public void MonitorOnlineStatus(int timeout = 5000)
        {
            cts = new CancellationTokenSource();
            Task.Run(() =>
            {
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        if (SearchResult != null && !cts.IsCancellationRequested && CanonicalName != null)
                        {
                            Ping(timeout);
                        }
                    }
                    catch
                    {
                        IsOnline = false;

                    }
                    Task.Delay(1000).Wait();
                }
            }, cts.Token);
            // await Task.Delay(1000);
            // MonitorOnlineStatus();

        }
        private void Ping(int timeout = 5000)
        {
            try
            {
                if (IPHostEntry == null && !cts.IsCancellationRequested && CanonicalName != null)
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
                        if (cts.IsCancellationRequested || CanonicalName == null) return;

                        PingReply response = ping.Send(CanonicalName, timeout);
                        if (response != null)
                        {
                            if (response.Status == IPStatus.Success)
                            {
                                IsOnline = true;
                                return;
                            }
                            else if (response.Status == IPStatus.TimedOut && x == retries - 1)
                            {
                                IsOnline = false;

                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Loggers.ActiveDirectryLogger.Error(ex.Message + " {@Error}", ex);
                    }
                    x++;
                } while (x < retries);

            }
            catch (SocketException)
            {
                // Ignore socket exceptions
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectryLogger.Error(ex.Message + " {@Error}", ex);

            }
        }

        public override void Dispose()
        {
            base.Dispose();
            cts.Cancel();
        }
    }
}
