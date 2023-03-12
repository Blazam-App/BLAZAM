using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using System.Management;

namespace BLAZAM.Common.Data.ActiveDirectory.Models
{
    internal class WmiConnection
    {
        private ManagementScope managementScope;

        public WmiConnection(ManagementScope managementScope)
        {
            this.managementScope = managementScope;
        }


        public List<IADComputerDrive> Drives
        {
            get
            {
                List<IADComputerDrive> drives = new List<IADComputerDrive>();
                try
                {



                    ObjectQuery query = new ObjectQuery("SELECT DeviceID,FreeSpace,Size,Description,DriveType,FileSystem,MediaType,VolumeDirty,VolumeSerialNumber FROM Win32_LogicalDisk");
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher(managementScope, query);
                    ManagementObjectCollection queryCollection = searcher.Get();

                    foreach (ManagementObject mo in queryCollection)
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
                            DriveType = driveType,
                            MediaType = mediaType
                        });
                        //Console.WriteLine("Free space: " + freeSpace + " GB");
                        //Console.WriteLine("Size: " + size + " GB");
                    }
                }catch(Exception ex)
                {
                    Loggers.ActiveDirectryLogger.Error("Error polling drives", ex);
                }
                return drives;
            }
        }

    }
}