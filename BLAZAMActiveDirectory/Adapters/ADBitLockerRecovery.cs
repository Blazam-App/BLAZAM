
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class ADBitLockerRecovery : DirectoryEntryAdapter, IADBitLockerRecovery
    {
        public override ActiveDirectoryObjectType ObjectType => ActiveDirectoryObjectType.BitLocker;


        public Guid? RecoveryId
        {
            get
            {
                var rawData = GetAttribute<byte[]>("msFVE-RecoveryGuid");
                if (rawData == null)
                {
                    return null;
                }
                var id = new Guid(rawData);
                return id;
            }


        }

        public string? RecoveryPassword
        {
            get
            {
                return GetStringAttribute("msFVE-RecoveryPassword");
            }
        }


    }
}
