using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Session.Interfaces;
using Microsoft.JSInterop;

namespace BLAZAM.Services.Audit
{
    public class ServerAuditLogger : BaseAuditLogger
    {
        public ServerAuditLogger(IAppDatabaseFactory factory) : base(factory, null)
        {

            System = new SystemAudit(factory);
            User = new UserAudit(factory);
            Group = new GroupAudit(factory);
            Computer = new ComputerAudit(factory);
            OU = new OUAudit(factory);
            Printer = new PrinterAudit(factory);
            BitLocker = new BitLockerAudit(factory);
            Email=new EmailAudit(factory);
        }


    }
}