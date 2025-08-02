using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Services.Events;
using BLAZAM.Session.Interfaces;
using Microsoft.JSInterop;
using Octokit;

namespace BLAZAM.Services.Audit
{
    public class BaseAuditLogger
    {
        public SystemAudit System { get; set; }
        public UserAudit User { get; set; }
        public GroupAudit Group { get; set; }
        public ComputerAudit Computer { get; set; }
        public OUAudit OU { get; set; }
        public PrinterAudit Printer { get; set; }
        public LogonAudit Logon { get; set; }
        public BitLockerAudit BitLocker { get; set; }
        public EmailAudit Email { get; set; }
        protected readonly IAppDatabaseFactory _factory;

        public BaseAuditLogger(IAppDatabaseFactory factory, IApplicationUserState userState)
        {
            _factory = factory;
            ApplicationEvents.DirectoryEntryChanged.Delegate += TriggerDirectoryEntryChangedEvent;
            System = new SystemAudit(factory);
            User = new UserAudit(factory, userState);
            Group = new GroupAudit(factory, userState);
            Computer = new ComputerAudit(factory, userState);
            OU = new OUAudit(factory, userState);
            Printer = new PrinterAudit(factory, userState);
            Logon = new LogonAudit(factory, userState);
            BitLocker = new BitLockerAudit(factory, userState);
        }

        protected static List<Guid> HandledEvents { get; set; } = new();

        public void ProcessDirectoryEntryChangedEvent(DirectoryEntryChangedArgs args)
        {
            lock (HandledEvents)
            {
                if (!HandledEvents.Contains(args.Guid))
                {
                    if (args.Actor != null)
                    {
                        User = new UserAudit(_factory, args.Actor);
                        Group = new GroupAudit(_factory, args.Actor);
                        Computer = new ComputerAudit(_factory, args.Actor);
                        OU = new OUAudit(_factory, args.Actor);
                        Printer = new PrinterAudit(_factory, args.Actor);
                        Logon = new LogonAudit(_factory, args.Actor);
                        BitLocker = new BitLockerAudit(_factory, args.Actor);
                    }
                    switch (args.ObjectType)
                    {
                        case ActiveDirectoryObjectType.User:
                            switch (args.EventType)
                            {
                                case ApplicationEventType.Create:
                                    User.Created(args.Entry);
                                    break;
                                case ApplicationEventType.Delete:
                                    User.Deleted(args.Entry);
                                    break;
                                case ApplicationEventType.Assign:
                                    User.Assigned(args.Entry, args.Target);
                                    Group.MemberAdded(args.Target, args.Entry);
                                    break;
                                case ApplicationEventType.Unassign:

                                    User.Unassigned(args.Entry, args.Target);
                                    Group.MemberRemoved(args.Target, args.Entry);
                                    break;
                                case ApplicationEventType.LockedOut:
                                    break;
                                case ApplicationEventType.Modify:
                                    User.Changed(args.Entry, args.Changes);
                                    break;
                                case ApplicationEventType.Move:
                                    User.Moved(args.Entry, args.Origin as IADOrganizationalUnit, args.Target as IADOrganizationalUnit);
                                    break;
                                case ApplicationEventType.PasswordChange:
                                    User.PasswordChanged(args.Entry, (args.Entry as IAccountDirectoryAdapter).RequirePasswordChange);
                                    break;
                                case ApplicationEventType.Search:
                                    User.Searched(args.Entry);
                                    break;
                            }
                            break;
                        case ActiveDirectoryObjectType.Printer:
                            switch (args.EventType)
                            {
                                case ApplicationEventType.Create:
                                    Printer.Created(args.Entry);
                                    break;
                                case ApplicationEventType.Delete:
                                    Printer.Deleted(args.Entry);
                                    break;
                                case ApplicationEventType.Assign:
                                    // Printer.Assigned(args.Entry, args.Target);
                                    Group.MemberAdded(args.Target, args.Entry);
                                    break;
                                case ApplicationEventType.Unassign:

                                    // Printer.Unassigned(args.Entry, args.Target);
                                    Group.MemberRemoved(args.Target, args.Entry);
                                    break;
                                case ApplicationEventType.LockedOut:
                                    break;
                                case ApplicationEventType.Modify:
                                    Printer.Changed(args.Entry, args.Changes);
                                    break;
                                case ApplicationEventType.Move:
                                    Printer.Moved(args.Entry, args.Origin as IADOrganizationalUnit, args.Target as IADOrganizationalUnit);
                                    break;
                                case ApplicationEventType.Search:
                                    Printer.Searched(args.Entry);
                                    break;

                            }
                            break;
                        case ActiveDirectoryObjectType.Computer:
                            switch (args.EventType)
                            {
                                case ApplicationEventType.Create:
                                    Computer.Created(args.Entry);
                                    break;
                                case ApplicationEventType.Delete:
                                    Computer.Deleted(args.Entry);
                                    break;
                                case ApplicationEventType.Assign:
                                    Computer.Assigned(args.Entry, args.Target);
                                    Group.MemberAdded(args.Target, args.Entry);
                                    break;
                                case ApplicationEventType.Unassign:

                                    Computer.Unassigned(args.Entry, args.Target);
                                    Group.MemberRemoved(args.Target, args.Entry);
                                    break;
                                case ApplicationEventType.LockedOut:
                                    break;
                                case ApplicationEventType.Modify:
                                    Computer.Changed(args.Entry, args.Changes);
                                    break;
                                case ApplicationEventType.Move:
                                    Computer.Moved(args.Entry, args.Origin as IADOrganizationalUnit, args.Target as IADOrganizationalUnit);
                                    break;
                                case ApplicationEventType.Search:
                                    Computer.Searched(args.Entry);
                                    break;
                            }
                            break;
                        case ActiveDirectoryObjectType.BitLocker:
                            switch (args.EventType)
                            {
                                case ApplicationEventType.Delete:
                                    BitLocker.Deleted(args.Entry);
                                    break;
                                case ApplicationEventType.Modify:
                                    BitLocker.Changed(args.Entry, args.Changes);
                                    break;
                            }
                            break;
                        case ActiveDirectoryObjectType.Group:
                            switch (args.EventType)
                            {
                                case ApplicationEventType.Create:
                                    Group.Created(args.Entry);
                                    break;
                                case ApplicationEventType.Delete:
                                    Group.Deleted(args.Entry);
                                    break;
                                case ApplicationEventType.Assign:
                                    Group.Assigned(args.Entry, args.Target);
                                    Group.MemberAdded(args.Target, args.Entry);
                                    break;
                                case ApplicationEventType.Unassign:

                                    Group.Unassigned(args.Entry, args.Target);
                                    Group.MemberRemoved(args.Target, args.Entry);
                                    break;
                                case ApplicationEventType.Modify:
                                    Group.Changed(args.Entry, args.Changes);
                                    break;
                                case ApplicationEventType.Move:
                                    Group.Moved(args.Entry, args.Origin as IADOrganizationalUnit, args.Target as IADOrganizationalUnit);
                                    break;
                                case ApplicationEventType.Search:
                                    Group.Searched(args.Entry);
                                    break;
                            }
                            break;
                        case ActiveDirectoryObjectType.OU:
                            switch (args.EventType)
                            {
                                case ApplicationEventType.Create:
                                    OU.Created(args.Entry);
                                    break;
                                case ApplicationEventType.Delete:
                                    OU.Deleted(args.Entry);
                                    break;

                                case ApplicationEventType.Modify:
                                    OU.Changed(args.Entry, args.Changes);
                                    break;
                                case ApplicationEventType.Move:
                                    OU.Moved(args.Entry, args.Origin as IADOrganizationalUnit, args.Target as IADOrganizationalUnit);
                                    break;
                                case ApplicationEventType.Search:
                                    OU.Searched(args.Entry);
                                    break;
                            }
                            break;
                    }

                    HandledEvents.Add(args.Guid);
                }

            }
        }

        protected virtual void TriggerDirectoryEntryChangedEvent(object? sender, DirectoryEntryChangedArgs args) => ProcessDirectoryEntryChangedEvent(args);

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