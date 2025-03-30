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
        public SystemAudit System;
        public UserAudit User;
        public GroupAudit Group;
        public ComputerAudit Computer;
        public OUAudit OU;
        public PrinterAudit Printer;
        public LogonAudit Logon;
        public BitLockerAudit BitLocker;
        public EmailAudit Email;

        public BaseAuditLogger(IAppDatabaseFactory factory, IApplicationUserStateService userStateService)
        {
            ApplicationEvents.DirectoryEntryChanged.Delegate += ProcessDirectoryEntryChangedEvent;
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
        public void ProcessDirectoryEntryChangedEvent(DirectoryEntryChangedArgs args)
        {
            lock (HandledEvents)
            {
                switch (args.ObjectType)
                {
                    case ActiveDirectoryObjectType.User:
                        switch (args.ActionType)
                        {
                            case AppEventType.Create:
                                User.Created(args.Entry);
                                break;
                            case AppEventType.Delete:
                                User.Deleted(args.Entry);
                                break;
                            case AppEventType.Assign:
                                User.Assigned(args.Entry, args.Target);
                                Group.MemberAdded(args.Target, args.Entry);
                                break;
                            case AppEventType.Unassign:

                                User.Unassigned(args.Entry, args.Target);
                                Group.MemberRemoved(args.Target, args.Entry);
                                break;
                            case AppEventType.LockedOut:
                                break;
                            case AppEventType.Modify:
                                User.Changed(args.Entry,args.Changes);
                                break;
                            case AppEventType.PasswordChange:
                                User.PasswordChanged(args.Entry,(args.Entry as IAccountDirectoryAdapter).RequirePasswordChange);
                                break;
                        }
                        break;
                    case ActiveDirectoryObjectType.Printer:
                        switch (args.ActionType)
                        {
                            case AppEventType.Create:
                                Printer.Created(args.Entry);
                                break;
                            case AppEventType.Delete:
                                Printer.Deleted(args.Entry);
                                break;
                            case AppEventType.Assign:
                               // Printer.Assigned(args.Entry, args.Target);
                                Group.MemberAdded(args.Target, args.Entry);
                                break;
                            case AppEventType.Unassign:

                               // Printer.Unassigned(args.Entry, args.Target);
                                Group.MemberRemoved(args.Target, args.Entry);
                                break;
                            case AppEventType.LockedOut:
                                break;
                            case AppEventType.Modify:
                                Printer.Changed(args.Entry, args.Changes);
                                break;
                          
                        }
                        break;
                    case ActiveDirectoryObjectType.Computer:
                        switch (args.ActionType)
                        {
                            case AppEventType.Create:
                                Computer.Created(args.Entry);
                                break;
                            case AppEventType.Delete:
                                Computer.Deleted(args.Entry);
                                break;
                            case AppEventType.Assign:
                                Computer.Assigned(args.Entry, args.Target);
                                Group.MemberAdded(args.Target, args.Entry);
                                break;
                            case AppEventType.Unassign:

                                Computer.Unassigned(args.Entry, args.Target);
                                Group.MemberRemoved(args.Target, args.Entry);
                                break;
                            case AppEventType.LockedOut:
                                break;
                            case AppEventType.Modify:
                                Computer.Changed(args.Entry, args.Changes);
                                break;
                          
                        }
                        break;
                    case ActiveDirectoryObjectType.BitLocker:
                        switch (args.ActionType)
                        {
                            case AppEventType.Delete:
                                BitLocker.Deleted(args.Entry);
                                break;
                            case AppEventType.Modify:
                                BitLocker.Changed(args.Entry, args.Changes);
                                break;
                        }
                        break;
                    case ActiveDirectoryObjectType.Group:
                        switch (args.ActionType)
                        {
                            case AppEventType.Create:
                                Group.Created(args.Entry);
                                break;
                            case AppEventType.Delete:
                                Group.Deleted(args.Entry);
                                break;
                            case AppEventType.Assign:
                                Group.Assigned(args.Entry, args.Target);
                                Group.MemberAdded(args.Target, args.Entry);
                                break;
                            case AppEventType.Unassign:

                                //Group.Unassigned(args.Entry, args.Target);
                                Group.MemberRemoved(args.Target, args.Entry);
                                break;
                            case AppEventType.Modify:
                                Group.Changed(args.Entry, args.Changes);
                                break;
                        }
                        break;
                    case ActiveDirectoryObjectType.OU:
                        switch (args.ActionType)
                        {
                            case AppEventType.Create:
                                OU.Created(args.Entry);
                                break;
                            case AppEventType.Delete:
                                OU.Deleted(args.Entry);
                                break;
                           
                            case AppEventType.Modify:
                                OU.Changed(args.Entry, args.Changes);
                                break;
                        }
                        break;
                }
                if (!HandledEvents.Contains(args.Guid))
                {
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

        public async Task Moved(IDirectoryEntryAdapter searchedEntry, IADOrganizationalUnit ouMovedFrom, IADOrganizationalUnit ouMovedTo)
        {
            if (searchedEntry is IADUser)
                await User.Moved(searchedEntry, ouMovedFrom, ouMovedTo);
            else if (searchedEntry is IADGroup)
                await Group.Moved(searchedEntry, ouMovedFrom, ouMovedTo);
            else if (searchedEntry is IADComputer)
                await Computer.Moved(searchedEntry, ouMovedFrom, ouMovedTo);
            else if (searchedEntry is IADOrganizationalUnit)
                await OU.Moved(searchedEntry, ouMovedFrom, ouMovedTo);
            else if (searchedEntry is IADPrinter)
                await Printer.Moved(searchedEntry, ouMovedFrom, ouMovedTo);
        }

    }
}