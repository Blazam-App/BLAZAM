
using BLAZAM.ActiveDirectory.Interfaces;

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


    }
}
