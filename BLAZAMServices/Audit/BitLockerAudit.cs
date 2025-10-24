using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Session.Interfaces;
using Microsoft.JSInterop;

namespace BLAZAM.Services.Audit
{
    public class BitLockerAudit : DirectoryAudit
    {
        public BitLockerAudit(IAppDatabaseFactory factory, IApplicationUserState? userState = null, IJSRuntime? jSRuntime = null) : base(factory, userState, jSRuntime)
        {
        }

        public override async Task<bool> Searched(IDirectoryEntryAdapter searchedEntry)
            => await Log(c => c.DirectoryEntryAuditLogs,
                AuditActions.BitLocker_Searched,
                searchedEntry);



    }
}