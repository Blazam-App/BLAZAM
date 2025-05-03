
using BLAZAM.ActiveDirectory.Interfaces;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class ADBitLockerRecovery : DirectoryEntryAdapter, IADBitLockerRecovery
    {


        public Guid? RecoveryId
        {
            get
            {
                var rawData = GetAttribute<byte[]>("msFVE-RecoveryGuid");
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
