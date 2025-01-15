using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Session.Interfaces;
using Microsoft.JSInterop;

namespace BLAZAM.Services.Audit
{
    public class BitLockerAudit : DirectoryAudit
    {
        public BitLockerAudit(IAppDatabaseFactory factory, IJSRuntime jSRuntime, IApplicationUserStateService userStateService) : base(factory, jSRuntime, userStateService)
        {
        }

        public override async Task<bool> Searched(IDirectoryEntryAdapter searchedOU)
            => await Log(c => c.DirectoryEntryAuditLogs,
                AuditActions.BitLocker_Searched,
                searchedOU);



    }
}