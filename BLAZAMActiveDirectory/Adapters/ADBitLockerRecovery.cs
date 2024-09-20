
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Models;
using BLAZAM.Logger;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class ADBitLockerRecovery : DirectoryEntryAdapter, IADBitLockerRecovery
    {


        public Guid? RecoveryId
        {
            get
            {
                var rawData = GetProperty<byte[]>("msFVE-RecoveryGuid");
                var id = new Guid(rawData);
                return id;
            }


        }

        public string? RecoveryPassword
        {
            get
            {
                return GetStringProperty("msFVE-RecoveryPassword");
            }
        }

        // public override ActiveDirectoryObjectType ObjectType =>  ActiveDirectoryObjectType.BitLocker;
    }
}
