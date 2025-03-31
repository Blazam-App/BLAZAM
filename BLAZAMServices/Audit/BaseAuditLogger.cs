using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Services.Events;
using BLAZAM.Session.Interfaces;
using Microsoft.JSInterop;

namespace BLAZAM.Services.Audit
{
    public class BaseAuditLogger
    {
        protected IApplicationUserStateService _userStateService;
        public SystemAudit System;
        public UserAudit User;
        public GroupAudit Group;
        public ComputerAudit Computer;
        public OUAudit OU;
        public PrinterAudit Printer;
        public LogonAudit Logon;
        public BitLockerAudit BitLocker;
        public EmailAudit Email;
        protected readonly IAppDatabaseFactory _factory;

        public BaseAuditLogger(IAppDatabaseFactory factory, IApplicationUserStateService userStateService)
        {
            _factory = factory;
            ApplicationEvents.DirectoryEntryChanged.Delegate += ProcessDirectoryEntryChangedEvent;
            _userStateService = userStateService;
            System = new SystemAudit(factory);
            User = new UserAudit(factory, userStateService);
            Group = new GroupAudit(factory, userStateService);
            Computer = new ComputerAudit(factory, userStateService);
            OU = new OUAudit(factory, userStateService);
            Printer = new PrinterAudit(factory, userStateService);
            Logon = new LogonAudit(factory, userStateService);
            BitLocker = new BitLockerAudit(factory, userStateService);
        }

        protected static List<Guid> HandledEvents { get; set; } = new();
        protected virtual void ProcessDirectoryEntryChangedEvent(object? sender, DirectoryEntryChangedArgs args)
        {
            lock (HandledEvents)
            {
                if (!HandledEvents.Contains(args.Guid))
                {

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