using BLAZAM.ActiveDirectory.Helpers;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using System.Management;

namespace BLAZAM.ActiveDirectory.Adapters
{
    internal class WmiConnection : IRemoteManagementConnection
    {
        private const string _driveStatsQuery = "SELECT DeviceID,FreeSpace,Size,Description,DriveType,FileSystem,MediaType,VolumeDirty,VolumeSerialNumber FROM Win32_LogicalDisk";
        private const string _totalMemoryQuery = "SELECT TotalVisibleMemorySize,FreePhysicalMemory FROM Win32_OperatingSystem";
        private const string _cpuStatsQuery = "SELECT * FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name='_Total'";
        //private const string _ipStatsQuery = "SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = 'TRUE'";
        private const string _servicesQuery = "SELECT * FROM Win32_Service";
        private const string _sharedPrintersQuery = "SELECT * FROM Win32_Printer";
        private readonly ManagementScope _managementScope;
        private readonly IADComputer _target;

        public WmiConnection(ManagementScope managementScope, IADComputer target)
        {
            this._managementScope = managementScope;
            this._target = target;
        }
        public async Task<bool> RenameComputerAsync(string newName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Use WMI to initiate the shutdown.
                    // The Win32_OperatingSystem class has a Win32Shutdown method.
                    SelectQuery query = new("Win32_ComputerSystem");
                    ManagementObjectSearcher searcher =
                        new(_managementScope, query);
                    var enumerator = searcher.Get().GetEnumerator();
                    var os = (ManagementObject)(enumerator.MoveNext() ? enumerator.Current : null);
                    if (os != null)
                    {
                        // Obtain in-parameters for the method
                        ManagementBaseObject inParams =
                            os.GetMethodParameters("Rename");

                        // Add the input parameters.
                        var username = _target.Directory.ConnectionSettings?.Username + "@" + _target.Directory.ConnectionSettings?.FQDN;
                        var pass = _target.Directory.ConnectionSettings?.Password.Decrypt<string>();
                        inParams["Name"] = newName;
                        inParams["UserName"] = username;
                        inParams["Password"] = pass;

                        // Execute the method and obtain the return values.
                        ManagementBaseObject outParams =
                            os.InvokeMethod("Rename", inParams, null);
                        var returnValue = (uint)outParams["ReturnValue"];
                        if (returnValue != 0)
                        {
                            throw new InvalidOperationException($"Failed to change computer name. Error code: {returnValue}");
                        }

                        return true;
                    }

                    Loggers.ActiveDirectoryLogger.Warning($"Rename command sent to {_target.CanonicalName}, but no Win32_OperatingSystem instance was found.");
                    return false; // No Win32_OperatingSystem object found.
                }
                catch (ManagementException mex)
                {
                    Loggers.ActiveDirectoryLogger.Error(mex, "Management exception while renaming {@CanonicalName}", _target.CanonicalName);
                    return false;
                }
                catch (UnauthorizedAccessException uaex)
                {
                    Loggers.ActiveDirectoryLogger.Error(uaex, "Unauthorized access while renaming {@CanonicalName}", _target.CanonicalName);
                    return false;
                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectoryLogger.Error(ex, "Exception while renaming {@Target}", _target?.CanonicalName);
                    return false;
                }
            });

        }
        public async Task<bool> ShutdownAsync(int delaySeconds = 0, string? message = null, bool force = true, bool reboot = false)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Use WMI to initiate the shutdown.
                    // The Win32_OperatingSystem class has a Win32Shutdown method.
                    SelectQuery query = new("Win32_OperatingSystem");
                    ManagementObjectSearcher searcher =
                        new(_managementScope, query);

                    foreach (ManagementObject os in searcher.Get().Cast<ManagementObject>())
                    {
                        // Obtain in-parameters for the method
                        ManagementBaseObject inParams =
                            os.GetMethodParameters("Win32ShutdownTracker");

                        // Add the input parameters.

                        // Shutdown flags
                        // https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32shutdown-method-in-class-win32-operatingsystem
                        int flags = 0;
                        if (force)
                        {
                            flags |= 4; // Force (adds to other flags) 0x4
                        }

                        if (reboot)
                        {
                            flags |= 2; // Reboot  0x2
                        }
                        else
                        {
                            flags |= 1; // Shutdown 0x1.  Logoff is 0x0, but *not* appropriate here
                        }

                        // Note:  If we added NO flags, it's a logoff.  Shutdown requires flags.
                        // Another flag, 0x8, is for power off (if supported).

                        inParams["Flags"] = flags;
                        inParams["Timeout"] = delaySeconds;
                        inParams["Comment"] = message;

#pragma warning disable S1751 // Loops with at most one iteration should be refactored
                        return true;
#pragma warning restore S1751 // Loops with at most one iteration should be refactored
                    }

                    Loggers.ActiveDirectoryLogger.Warning("Shutdown command sent to {CanonicalName}, but no Win32_OperatingSystem instance was found.", _target.CanonicalName);
                    return false; // No Win32_OperatingSystem object found.
                }
                catch (ManagementException mex)
                {
                    Loggers.ActiveDirectoryLogger.Error(mex, "Management exception while shutting down {CanonicalName}", _target.CanonicalName);
                    return false;
                }
                catch (UnauthorizedAccessException uaex)
                {
                    Loggers.ActiveDirectoryLogger.Error(uaex, "Unauthorized access while shutting down {CanonicalName}", _target.CanonicalName);
                    return false;
                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectoryLogger.Error(ex, "Exception while shutting down {CanonicalName}", _target.CanonicalName);
                    return false;
                }
            });
        }
        public ComputerMemory Memory
        {
            get
            {
                foreach (var mo in PerformQuery(_totalMemoryQuery))
                {
                    double total = Convert.ToDouble(mo["TotalVisibleMemorySize"]);
                    double free = Convert.ToDouble(mo["FreePhysicalMemory"]);
                    return new ComputerMemory { Total = total, Free = free };
                }

                return new ComputerMemory();

            }
        }

        public List<ComputerService> Services
        {
            get
            {
                List<ComputerService> services = [];
                foreach (var mo in PerformQuery(_servicesQuery))
                {
                    ComputerService service = new()
                    {
                        Status = mo.GetPropertyValue<string>("Status"),
                        State = mo.GetPropertyValue<string>("State"),
                        Caption = mo.GetPropertyValue<string>("Caption"),
                        DisplayName = mo.GetPropertyValue<string>("DisplayName"),
                        Description = mo.GetPropertyValue<string>("Description"),
                        ErrorControl = mo.GetPropertyValue<string>("ErrorControl"),
                        ServiceType = mo.GetPropertyValue<string>("ServiceType"),
                        PathName = mo.GetPropertyValue<string>("PathName"),
                        Name = mo.GetPropertyValue<string>("Name"),
                        StartMode = mo.GetPropertyValue<string>("StartMode"),
                        StartName = mo.GetPropertyValue<string>("StartName"),
                        InstallDate = mo.GetPropertyValue<DateTime>("InstallDate"),
                        CanPause = mo.GetPropertyValue<bool>("AcceptPause"),
                        CanStop = mo.GetPropertyValue<bool>("AcceptStop"),
                        Started = mo.GetPropertyValue<bool>("Started")
                    };
                    services.Add(service);
                }

                return services;
            }
        }

        public int Processor
        {
            get
            {
                foreach (var mo in PerformQuery(_cpuStatsQuery))
                {
                    int percentProcessor = Convert.ToInt32(mo["PercentProcessorTime"]);
                    return percentProcessor;
                }

                return 0;
            }
        }

        public List<IADComputerDrive> Drives
        {
            get
            {
                List<IADComputerDrive> drives = [];
                try
                {

                    foreach (var mo in PerformQuery(_driveStatsQuery))
                    {
                        string? letter = mo["DeviceID"]?.ToString();
                        string? description = mo["Description"]?.ToString();
                        string? fileSystem = mo["FileSystem"]?.ToString();
                        bool volumeDirty = Convert.ToBoolean(mo["VolumeDirty"]);
                        string? volumeSerial = mo["VolumeSerialNumber"]?.ToString();
                        int driveType = Convert.ToInt32(mo["DriveType"]);
                        int mediaType = Convert.ToInt32(mo["MediaType"]);
                        double freeSpace = Convert.ToDouble(mo["FreeSpace"]) / (1024 * 1024 * 1024);
                        double size = Convert.ToDouble(mo["Size"]) / (1024 * 1024 * 1024);
                        drives.Add(new ADComputerDrive
                        {
                            Letter = letter,
                            Capacity = size,
                            FreeSpace = freeSpace,
                            Description = description,
                            FileSystem = fileSystem,
                            Dirty = volumeDirty,
                            Serial = volumeSerial,
                            DriveType = (DriveType)driveType,
                            MediaType = mediaType
                        });

                    }
                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectoryLogger.Error(ex, "Error polling drives");
                }

                return drives;
            }
        }

        public List<SharedPrinter> SharePrinters
        {
            get
            {
                List<SharedPrinter> sharedPrinters = [];
                try
                {

                    foreach (var mo in PerformQuery(_sharedPrintersQuery))
                    {
                        if ((bool)mo["Shared"])
                        {

                            sharedPrinters.Add(new SharedPrinter(_target, mo));

                        }
                    }
                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectoryLogger.Error(ex, "Error polling printers");
                }



                return sharedPrinters;
            }
        }

        private List<ManagementObject> PerformQuery(string query)
        {
            try
            {
                List<ManagementObject> results = [];
                ObjectQuery objectQuery = new(query);
                ManagementObjectSearcher searcher = new(_managementScope, objectQuery);
                ManagementObjectCollection queryCollection = searcher.Get();
                if (queryCollection.Count > 0)
                {
                    foreach (ManagementObject mo in queryCollection.Cast<ManagementObject>())
                    {
                        results.Add(mo);
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Information(ex, "WMI query failure");

            }

            return [];
        }
    }
    public class ComputerService
    {
        public bool? CanPause { get; set; }
        public bool? CanStop { get; set; }
        public bool? Started { get; set; }
        public string? StartMode { get; set; }
        public string? StartName { get; set; }
        public string? State { get; set; }
        public string? Status { get; set; }
        public string? Name { get; set; }
        public string? PathName { get; set; }
        public UInt32? ProcessId { get; set; }
        public string? Caption { get; set; }
        public string? ServiceType { get; set; }
        public string? Description { get; set; }
        public string? DisplayName { get; set; }
        public string? ErrorControl { get; set; }
        public UInt32? ExitCode { get; set; }
        public DateTime? InstallDate { get; set; }
        public bool? DelayedAutoStart { get; set; }

    }
}