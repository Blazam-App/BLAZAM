using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Context;
using BLAZAM.Services.Events;
using BLAZAM.Session.Interfaces;

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
                // Exit early if this event has already been processed to prevent duplicates.
                if (HandledEvents.Contains(args.Guid))
                {
                    return;
                }

                // If an actor is associated with the event, initialize the audit services
                // that will log the changes.
                if (args.Actor != null)
                {
                    InitializeAuditServices(args.Actor);
                }

                // Delegate the specific event processing based on the type of Active Directory object.
                DispatchAuditEvent(args);

                // Mark the event as handled.
                HandledEvents.Add(args.Guid);
            }
        }

        /// <summary>
        /// Creates instances of all audit services for a given actor.
        /// </summary>
        private void InitializeAuditServices(IApplicationUserState actor)
        {
            User = new UserAudit(_factory, actor);
            Group = new GroupAudit(_factory, actor);
            Computer = new ComputerAudit(_factory, actor);
            OU = new OUAudit(_factory, actor);
            Printer = new PrinterAudit(_factory, actor);
            Logon = new LogonAudit(_factory, actor);
            BitLocker = new BitLockerAudit(_factory, actor);
        }

        /// <summary>
        /// Routes the directory change event to the appropriate handler based on the object type.
        /// </summary>
        private void DispatchAuditEvent(DirectoryEntryChangedArgs args)
        {
            switch (args.ObjectType)
            {
                case ActiveDirectoryObjectType.User:
                    ProcessUserChangeEvent(args);
                    break;
                case ActiveDirectoryObjectType.Printer:
                    ProcessPrinterChangeEvent(args);
                    break;
                case ActiveDirectoryObjectType.Computer:
                    ProcessComputerChangeEvent(args);
                    break;
                case ActiveDirectoryObjectType.BitLocker:
                    ProcessBitLockerChangeEvent(args);
                    break;
                case ActiveDirectoryObjectType.Group:
                    ProcessGroupChangeEvent(args);
                    break;
                case ActiveDirectoryObjectType.OU:
                    ProcessOUChangeEvent(args);
                    break;
            }
        }

        /// <summary>
        /// Handles audit logging for user-related directory changes.
        /// </summary>
        private void ProcessUserChangeEvent(DirectoryEntryChangedArgs args)
        {
            switch (args.EventType)
            {
                case ApplicationEventType.Create:
                    _ = User.Created(args.Entry);
                    break;
                case ApplicationEventType.Delete:
                    _ = User.Deleted(args.Entry);
                    break;
                case ApplicationEventType.Assign:
                    _ = User.Assigned(args.Entry, args.Target);
                    _ = Group.MemberAdded(args.Target, args.Entry);
                    break;
                case ApplicationEventType.Unassign:
                    _ = User.Unassigned(args.Entry, args.Target);
                    _ = Group.MemberRemoved(args.Target, args.Entry);
                    break;
                case ApplicationEventType.Modify:
                    _ = User.Changed(args.Entry, args.Changes);
                    break;
                case ApplicationEventType.Move:
                    _ = User.Moved(args.Entry, args.Origin as IADOrganizationalUnit, args.Target as IADOrganizationalUnit);
                    break;
                case ApplicationEventType.PasswordChange:
                    _ = User.PasswordChanged(args.Entry, (args.Entry as IAccountDirectoryAdapter).RequirePasswordChange);
                    break;
                case ApplicationEventType.Search:
                    _ = User.Searched(args.Entry);
                    break;
                case ApplicationEventType.LockedOut:
                    break; // No action needed.
            }
        }

        /// <summary>
        /// Handles audit logging for printer-related directory changes.
        /// </summary>
        private void ProcessPrinterChangeEvent(DirectoryEntryChangedArgs args)
        {
            switch (args.EventType)
            {
                case ApplicationEventType.Create:
                    _ = Printer.Created(args.Entry);
                    break;
                case ApplicationEventType.Delete:
                    _ = Printer.Deleted(args.Entry);
                    break;
                case ApplicationEventType.Assign:
                    // Printer assignment is tracked via group membership changes.
                    _ = Group.MemberAdded(args.Target, args.Entry);
                    break;
                case ApplicationEventType.Unassign:
                    // Printer unassignment is tracked via group membership changes.
                    _ = Group.MemberRemoved(args.Target, args.Entry);
                    break;
                case ApplicationEventType.Modify:
                    _ = Printer.Changed(args.Entry, args.Changes);
                    break;
                case ApplicationEventType.Move:
                    _ = Printer.Moved(args.Entry, args.Origin as IADOrganizationalUnit, args.Target as IADOrganizationalUnit);
                    break;
                case ApplicationEventType.Search:
                    _ = Printer.Searched(args.Entry);
                    break;
                case ApplicationEventType.LockedOut:
                    break; // Not applicable to printers.
            }
        }

        /// <summary>
        /// Handles audit logging for computer-related directory changes.
        /// </summary>
        private void ProcessComputerChangeEvent(DirectoryEntryChangedArgs args)
        {
            switch (args.EventType)
            {
                case ApplicationEventType.Create:
                    _ = Computer.Created(args.Entry);
                    break;
                case ApplicationEventType.Delete:
                    _ = Computer.Deleted(args.Entry);
                    break;
                case ApplicationEventType.Assign:
                    _ = Computer.Assigned(args.Entry, args.Target);
                    _ = Group.MemberAdded(args.Target, args.Entry);
                    break;
                case ApplicationEventType.Unassign:
                    _ = Computer.Unassigned(args.Entry, args.Target);
                    _ = Group.MemberRemoved(args.Target, args.Entry);
                    break;
                case ApplicationEventType.Modify:
                    _ = Computer.Changed(args.Entry, args.Changes);
                    break;
                case ApplicationEventType.Move:
                    _ = Computer.Moved(args.Entry, args.Origin as IADOrganizationalUnit, args.Target as IADOrganizationalUnit);
                    break;
                case ApplicationEventType.Search:
                    _ = Computer.Searched(args.Entry);
                    break;
                case ApplicationEventType.LockedOut:
                    break; // Not applicable to computers.
            }
        }

        /// <summary>
        /// Handles audit logging for BitLocker-related directory changes.
        /// </summary>
        private void ProcessBitLockerChangeEvent(DirectoryEntryChangedArgs args)
        {
            switch (args.EventType)
            {
                case ApplicationEventType.Delete:
                    _ = BitLocker.Deleted(args.Entry);
                    break;
                case ApplicationEventType.Modify:
                    _ = BitLocker.Changed(args.Entry, args.Changes);
                    break;
            }
        }

        /// <summary>
        /// Handles audit logging for group-related directory changes.
        /// </summary>
        private void ProcessGroupChangeEvent(DirectoryEntryChangedArgs args)
        {
            switch (args.EventType)
            {
                case ApplicationEventType.Create:
                    _ = Group.Created(args.Entry);
                    break;
                case ApplicationEventType.Delete:
                    _ = Group.Deleted(args.Entry);
                    break;
                case ApplicationEventType.Assign:
                    _ = Group.Assigned(args.Entry, args.Target);
                    _ = Group.MemberAdded(args.Target, args.Entry);
                    break;
                case ApplicationEventType.Unassign:
                    _ = Group.Unassigned(args.Entry, args.Target);
                    _ = Group.MemberRemoved(args.Target, args.Entry);
                    break;
                case ApplicationEventType.Modify:
                    _ = Group.Changed(args.Entry, args.Changes);
                    break;
                case ApplicationEventType.Move:
                    _ = Group.Moved(args.Entry, args.Origin as IADOrganizationalUnit, args.Target as IADOrganizationalUnit);
                    break;
                case ApplicationEventType.Search:
                    _ = Group.Searched(args.Entry);
                    break;
            }
        }

        /// <summary>
        /// Handles audit logging for Organizational Unit (OU) related directory changes.
        /// </summary>
        private void ProcessOUChangeEvent(DirectoryEntryChangedArgs args)
        {
            switch (args.EventType)
            {
                case ApplicationEventType.Create:
                    _ = OU.Created(args.Entry);
                    break;
                case ApplicationEventType.Delete:
                    _ = OU.Deleted(args.Entry);
                    break;
                case ApplicationEventType.Modify:
                    _ = OU.Changed(args.Entry, args.Changes);
                    break;
                case ApplicationEventType.Move:
                    _ = OU.Moved(args.Entry, args.Origin as IADOrganizationalUnit, args.Target as IADOrganizationalUnit);
                    break;
                case ApplicationEventType.Search:
                    _ = OU.Searched(args.Entry);
                    break;
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