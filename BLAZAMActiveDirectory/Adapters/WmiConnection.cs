using System.Management;
using BLAZAM.ActiveDirectory.Helpers;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Helpers;
using BLAZAM.Logger;

namespace BLAZAM.ActiveDirectory.Adapters
{
    internal class WmiConnection
    {
        private const string DriveStatsQuery = "SELECT DeviceID,FreeSpace,Size,Description,DriveType,FileSystem,MediaType,VolumeDirty,VolumeSerialNumber FROM Win32_LogicalDisk";
        private const string TotalMemoryQuery = "SELECT TotalVisibleMemorySize,FreePhysicalMemory FROM Win32_OperatingSystem";
        private const string CPUStatsQuery = "SELECT * FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name='_Total'";
        private const string IPStatsQuery = "SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = 'TRUE'";
        private const string ServicesQuery = "SELECT * FROM Win32_Service";
        private const string SharedPrintersQuery = "SELECT * FROM Win32_Printer";
        private ManagementScope managementScope;
        private IADComputer target;

        public WmiConnection(ManagementScope managementScope, IADComputer target)
        {
            this.managementScope = managementScope;
            this.target = target;
        }
        public async Task<bool> RenameComputerAsync(string newName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Use WMI to initiate the shutdown.
                    // The Win32_OperatingSystem class has a Win32Shutdown method.
                    SelectQuery query = new SelectQuery("Win32_ComputerSystem");
                    ManagementObjectSearcher searcher =
                        new ManagementObjectSearcher(managementScope, query);

                    foreach (ManagementObject os in searcher.Get())
                    {
                        // Obtain in-parameters for the method
                        ManagementBaseObject inParams =
                            os.GetMethodParameters("Rename");

                        // Add the input parameters.
                        var username = target.Directory.ConnectionSettings.Username + "@" + target.Directory.ConnectionSettings.FQDN;
                        var pass = target.Directory.ConnectionSettings.Password.Decrypt<string>();
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
#pragma warning disable S1751 // Loops with at most one iteration should be refactored
                        return true;
#pragma warning restore S1751 // Loops with at most one iteration should be refactored
                    }

                    Loggers.ActiveDirectoryLogger.Warning($"Rename command sent to {target.CanonicalName}, but no Win32_OperatingSystem instance was found.");
                    return false; // No Win32_OperatingSystem object found.
                }
                catch (ManagementException mex)
                {
                    Loggers.ActiveDirectoryLogger.Error($"Management exception while renaming {target.CanonicalName}: {mex.Message} ErrorCode: {mex.ErrorCode}", mex);
                    return false;
                }
                catch (UnauthorizedAccessException uaex)
                {
                    Loggers.ActiveDirectoryLogger.Error($"Unauthorized access while renaming {target.CanonicalName}: {uaex.Message}", uaex);
                    return false;
                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectoryLogger.Error(ex, "Exception while renaming {@Target}", target?.CanonicalName);
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
                    SelectQuery query = new SelectQuery("Win32_OperatingSystem");
                    ManagementObjectSearcher searcher =
                        new ManagementObjectSearcher(managementScope, query);

                    foreach (ManagementObject os in searcher.Get())
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

                        // Execute the method and obtain the return values.
                        ManagementBaseObject outParams =
                            os.InvokeMethod("Win32ShutdownTracker", inParams, null);
#pragma warning disable S1751 // Loops with at most one iteration should be refactored
                        return true;
#pragma warning restore S1751 // Loops with at most one iteration should be refactored
                    }

                    Loggers.ActiveDirectoryLogger.Warning($"Shutdown command sent to {target.CanonicalName}, but no Win32_OperatingSystem instance was found.");
                    return false; // No Win32_OperatingSystem object found.
                }
                catch (ManagementException mex)
                {
                    Loggers.ActiveDirectoryLogger.Error($"Management exception while shutting down {target.CanonicalName}: {mex.Message} ErrorCode: {mex.ErrorCode}", mex);
                    return false;
                }
                catch (UnauthorizedAccessException uaex)
                {
                    Loggers.ActiveDirectoryLogger.Error($"Unauthorized access while shutting down {target.CanonicalName}: {uaex.Message}", uaex);
                    return false;
                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectoryLogger.Error($"Exception while shutting down {target.CanonicalName}: {ex.Message}", ex);
                    return false;
                }
            });



        }
        public ComputerMemory Memory
        {
            get
            {
                foreach (var mo in PerformQuery(TotalMemoryQuery))
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
                List<ComputerService> services = new();
                foreach (var mo in PerformQuery(ServicesQuery))
                {
                    ComputerService service = new();
                    service.Status = mo.GetPropertyValue<string>("Status");
                    service.State = mo.GetPropertyValue<string>("State");
                    service.Caption = mo.GetPropertyValue<string>("Caption");
                    service.DisplayName = mo.GetPropertyValue<string>("DisplayName");
                    service.Description = mo.GetPropertyValue<string>("Description");
                    service.ErrorControl = mo.GetPropertyValue<string>("ErrorControl");
                    service.ServiceType = mo.GetPropertyValue<string>("ServiceType");
                    service.PathName = mo.GetPropertyValue<string>("PathName");
                    service.Name = mo.GetPropertyValue<string>("Name");
                    service.StartMode = mo.GetPropertyValue<string>("StartMode");
                    service.StartName = mo.GetPropertyValue<string>("StartName");
                    service.InstallDate = mo.GetPropertyValue<DateTime>("InstallDate");
                    service.CanPause = mo.GetPropertyValue<bool>("AcceptPause");
                    service.CanStop = mo.GetPropertyValue<bool>("AcceptStop");
                    service.Started = mo.GetPropertyValue<bool>("Started");
                    services.Add(service);
                }
                return services;
            }
        }

        public int Processor
        {
            get
            {
                foreach (var mo in PerformQuery(CPUStatsQuery))
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
                List<IADComputerDrive> drives = new();
                try
                {

                    foreach (var mo in PerformQuery(DriveStatsQuery))
                    {
                        string letter = mo["DeviceID"]?.ToString();
                        string description = mo["Description"]?.ToString();
                        string fileSystem = mo["FileSystem"]?.ToString();
                        bool volumeDirty = Convert.ToBoolean(mo["VolumeDirty"]);
                        string volumeSerial = mo["VolumeSerialNumber"]?.ToString();
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
                List<SharedPrinter> sharedPrinters = new();
                try
                {

                    foreach (var mo in PerformQuery(SharedPrintersQuery))
                    {
                        if ((bool)mo["Shared"])
                        {

                            sharedPrinters.Add(new SharedPrinter(target, mo));

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
                List<ManagementObject> results = new();
                ObjectQuery objectQuery = new(query);
                ManagementObjectSearcher searcher = new(managementScope, objectQuery);
                ManagementObjectCollection queryCollection = searcher.Get();
                if (queryCollection.Count > 0)
                {
                    foreach (ManagementObject mo in queryCollection)
                    {
                        results.Add(mo);
                    }
                }
                return results;
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Error("WMI query failure: " + ex.Message);

            }
            return new();
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