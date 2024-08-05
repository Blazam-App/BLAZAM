

using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Session.Interfaces;
using Serilog.Parsing;
using System.Threading.Channels;

namespace BLAZAM.Services.Audit
{
    public class AuditLogger
    {


        public SystemAudit System;
        public UserAudit User;
        public GroupAudit Group;
        public ComputerAudit Computer;
        public OUAudit OU;
        public PrinterAudit Printer;
        public LogonAudit Logon;
        public BitLockerAudit BitLocker;
        public AuditLogger(IAppDatabaseFactory factory, IApplicationUserStateService userStateService)
        {
            System = new SystemAudit(factory);
            User = new UserAudit(factory, userStateService);
            Group = new GroupAudit(factory, userStateService);
            Computer = new ComputerAudit(factory, userStateService);
            OU = new OUAudit(factory, userStateService);
            Printer = new PrinterAudit(factory, userStateService);
            Logon = new LogonAudit(factory, userStateService);
            BitLocker = new BitLockerAudit(factory, userStateService);
        }
        public async Task Searched(IDirectoryEntryAdapter searchedEntry)
        {
            if (searchedEntry is IADUser)
                await User.Searched(searchedEntry);
            else if (searchedEntry is IADGroup)
                await Group.Searched(searchedEntry);
            else if (searchedEntry is IADComputer)
                await Computer.Searched(searchedEntry);
            else if (searchedEntry is IADOrganizationalUnit)
                await OU.Searched(searchedEntry);
            else if (searchedEntry is IADPrinter)
                await Printer.Searched(searchedEntry);
            else if (searchedEntry is IADBitLockerRecovery)
                await BitLocker.Searched(searchedEntry);
        }

    }
}