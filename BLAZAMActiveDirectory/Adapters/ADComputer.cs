
using BLAZAM.ActiveDirectory.Data;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using Newtonsoft.Json;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class ADComputer : AccountDirectoryAdapter, IADComputer
    {

        private ADComputerSessions? sessionManager;
        private WmiConnection? _wmiConnection;
        private WmiConnection? wmiConnection
        {
            get
            {
                if (CanonicalName == null) return null;
                if (_wmiConnection == null)
                {
                    _wmiConnection = new WmiConnection(Directory.Computers.WmiFactory.CreateWmiConnection(CanonicalName), this);
                }
                return _wmiConnection;
            }
        }
        private CancellationTokenSource _pingCancellationTokenSource = new();
        private bool? online;
        public ADComputer()
        {

        }

        public async Task<List<IADBitLockerRecovery>?> GetBitLockerRecoveryAsync()
        {
            var recovery = await Directory.BitLocker.FindByComputerAsync(this);
            return recovery;
        }
        public async Task<bool> RenameAsync(string newName)
        {
            if (string.IsNullOrEmpty(CanonicalName))
            {
                Loggers.ActiveDirectoryLogger.Warning("Cannot rename computer. CanonicalName is null or empty.");
                return false;
            }


            if (wmiConnection != null)
            {
                return await wmiConnection.RenameComputerAsync(newName);
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> ShutdownAsync(int delaySeconds = 0, string? message = null, bool force = true, bool reboot = false)
        {
            if (string.IsNullOrEmpty(CanonicalName))
            {
                Loggers.ActiveDirectoryLogger.Warning("Cannot shutdown computer. CanonicalName is null or empty.");
                return false;
            }

            if (delaySeconds < 0)
            {
                Loggers.ActiveDirectoryLogger.Warning("Delay cannot be negative. Setting to 0");
                delaySeconds = 0; //Prevent exceptions
            }
            if (wmiConnection != null)
            {
                return await wmiConnection.ShutdownAsync(delaySeconds, message, force, reboot);
            }
            else
            {
                return false;
            }
        }
        public string? OperatingSystem
        {
            get
            {
                return GetStringAttribute(ActiveDirectoryFields.OperatingSystem.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.OperatingSystem.FieldName, value);
            }

        }
        private LapsCredential? _lapsCredential;
        public SecureString? LapsPassword
        {
            get
            {
                if (_lapsCredential != null) return _lapsCredential.Password;
                return GetLapsCredentials()?.Password;
            }
        }
        public SecureString? LapsUsername
        {
            get
            {
                if (_lapsCredential != null) return _lapsCredential.AccountName;
                return GetLapsCredentials()?.AccountName;
            }
        }

        private LapsCredential? GetLapsCredentials()
        {
            string? pass = null;
            object? value = null;
            if (System.OperatingSystem.IsWindows())
            {
                value = GetAttribute<object>("msLAPS-EncryptedPassword");
                if (value is byte[] bytes)
                {
                    try
                    {
                        return Directory.Impersonation.Run(() =>
                        {
                            string decryptedPassword = String.Empty;
                            var decryptor = new LapsDecryptor();
                            var decryptedJson = decryptor.Decrypt(bytes).ToSecureString();
                            //decryptedPassword = decryptedJson;
                            _lapsCredential = new LapsCredential(decryptedJson);

                            return _lapsCredential;
                        });

                    }
                    catch (Exception ex)
                    {
                        Loggers.ActiveDirectoryLogger.Information(ex, "Error decrypting LAPS password for {@Computer}", DN);
                    }
                }
            }
            if (value is null)
            {
                var str = GetSecureStringAttribute(ActiveDirectoryFields.LapsPassword.FieldName);
                if (str != null)
                {
                    _lapsCredential = new(str);
                    return _lapsCredential;
                }
            }
            return null;
        }

        private SecureString? GetSecureStringAttribute(string fieldName)
        {
            var temp = GetStringAttribute(fieldName);
            if (temp != null) return temp.ToSecureString();
            return null;
        }

        public DateTime? LapsPasswordExpiration
        {
            get
            {
                return GetDateTimeAttribute("msLAPS-PasswordExpirationTime");
            }
            set
            {
                SetFileTimeAttribute("msLAPS-PasswordExpirationTime", value);
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
        public AppDelegate<bool> OnOnlineChanged { get; set; }

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

                return false;

            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Error(ex, "Error renaming computer");
            }
            return false;

        }



        /// <summary>
        /// When called this computers network reachability will be continuously monitored.
        /// </summary>
        /// <param name="timeout"></param>
        public void MonitorOnlineStatus(int timeout = 5000)
        {
            if (_pingCancellationTokenSource == null || _pingCancellationTokenSource.IsCancellationRequested)
                _pingCancellationTokenSource = new CancellationTokenSource();
            Task.Run(() =>
            {
                while (!_pingCancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        if (SearchResult != null && !_pingCancellationTokenSource.IsCancellationRequested && CanonicalName != null)
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
                _pingCancellationTokenSource.Dispose();
            }, _pingCancellationTokenSource.Token);

        }
        private void Ping(int timeout = 5000)
        {
            try
            {
                if (IPHostEntry == null && !_pingCancellationTokenSource.IsCancellationRequested && CanonicalName != null)
                {
                    IPHostEntry = Dns.GetHostEntry(CanonicalName);
                    Task.Delay(60000).ContinueWith((s) =>
                    {
                        IPHostEntry = null;
                    });
                }
                Ping ping = new();
                int retries = 5;
                int x = 0;
                do
                {
                    try
                    {
                        if (_pingCancellationTokenSource.IsCancellationRequested || CanonicalName == null) return;

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
                        Loggers.ActiveDirectoryLogger.Error(ex, "Error pinging computer");
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
                Loggers.ActiveDirectoryLogger.Error(ex, "Error looking up DNS info for computer");

            }
        }

        public override void Dispose()
        {
            _pingCancellationTokenSource.Cancel();
            sessionManager?.Dispose();
            base.Dispose();
        }
    }
}
