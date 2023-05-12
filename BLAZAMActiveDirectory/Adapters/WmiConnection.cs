using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using System.Management;

namespace BLAZAM.ActiveDirectory.Adapters
{
    internal class WmiConnection
    {
        private const string DriveStatsQuery = "SELECT DeviceID,FreeSpace,Size,Description,DriveType,FileSystem,MediaType,VolumeDirty,VolumeSerialNumber FROM Win32_LogicalDisk";
        private const string TotalMemoryQuery = "SELECT TotalVisibleMemorySize,FreePhysicalMemory FROM Win32_OperatingSystem";
        private const string CPUStatsQuery = "SELECT * FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name='_Total'";
        private const string IPStatsQuery = "SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = 'TRUE'";
        private const string ServicesQuery = "SELECT * FROM Win32_Service";
        private ManagementScope managementScope;

        public WmiConnection(ManagementScope managementScope)
        {
            this.managementScope = managementScope;
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
                };

                return new ComputerMemory();

            }
        }

        public List<ComputerService> Services
        {
            get
            {
                List<ComputerService> services = new List<ComputerService>();
                foreach (var mo in PerformQuery(ServicesQuery))
                {
                    ComputerService service = new ComputerService();
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
                    service.InstallDate = mo.GetPropertyValue<DateTime>("AcceptPause");
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
                var test = mo;
                int percentIdle = Convert.ToInt32(mo["PercentIdleTime"]);
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
            List<IADComputerDrive> drives = new List<IADComputerDrive>();
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
                    //Console.WriteLine("Free space: " + freeSpace + " GB");
                    //Console.WriteLine("Size: " + size + " GB");
                }
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectryLogger.Error("Error polling drives", ex);
            }
            return drives;
        }
    }

    private List<ManagementObject> PerformQuery(string query)
    {
            try
            {
                List<ManagementObject> results = new();
                ObjectQuery objectQuery = new ObjectQuery(query);
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(managementScope, objectQuery);
                ManagementObjectCollection queryCollection = searcher.Get();
                if (queryCollection.Count > 0)
                {
                    foreach (ManagementObject mo in queryCollection)
                    {
                        results.Add(mo);
                    }
                }
                return results;
            }catch(Exception ex)
            {
                Loggers.ActiveDirectryLogger.Error("WMI query failure: " + ex.Message);

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
        public DateTime? InstallDate{ get; set; }
        public bool? DelayedAutoStart { get; set; }

    }
}