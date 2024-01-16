

using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Helpers;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models.Audit;
using BLAZAM.Session.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog.Parsing;
using System.Threading.Channels;
using BLAZAM.Logger;

namespace BLAZAM.Services
{
    public class AuditActions
    {

        //User Actions
        public const string Password_Changed = "Password Changed";


        public const string User_Searched = "User Searched";
        public const string User_Enabled = "User Enabled";
        public const string User_Disabled = "User Disabled";
        public const string User_Assigned = "User Assigned";
        public const string User_Unassigned = "User Unassigned";
        public const string User_Unlocked = "User Unlocked";
        public const string User_Created = "User Created";
        public const string User_Deleted = "User Deleted";
        public const string User_Moved = "User Moved";
        public const string User_Edited = "User Edited";

        //Computer Actions
        public const string Computer_Searched = "Computer Searched";
        public const string Computer_Enabled = "Computer Enabled";
        public const string Computer_Disabled = "Computer Disabled";
        public const string Computer_Assigned = "Computer Assigned";
        public const string Computer_Unassigned = "Computer Unassigned";
        public const string Computer_Unlocked = "Computer Unlocked";
        public const string Computer_Created = "Computer Created";
        public const string Computer_Deleted = "Computer Deleted";
        public const string Computer_Moved = "Computer Moved";
        public const string Computer_Edited = "Computer Edited";


        //Group Actions
        public const string Group_Searched = "Group Searched";
        public const string Group_Assigned = "Group Assigned";
        public const string Group_Unassigned = "Group Unassigned";
        public const string Group_Created = "Group Created";
        public const string Group_Deleted = "Group Deleted";
        public const string Group_Moved = "Group Moved";
        public const string Group_Edited = "Group Edited";


        //OU Actions
        public const string OU_Searched = "OU Searched";
        public const string OU_Created = "OU Created";
        public const string OU_Deleted = "OU Deleted";
        public const string OU_Moved = "OU Moved";
        public const string OU_Edited = "OU Edited";

        //Settings Actions
        public const string Settings_Edited = "Settings Edited";

        //Permissions Actions
        public const string Permission_Group_Added = "Group Added";
        public const string Permission_Level_Added = "Level Added";
        public const string Permission_Mapping_Added = "Mapping Added";
        public const string Permission_Group_Edited = "Group Edited";
        public const string Permission_Level_Edited = "Level Edited";
        public const string Permission_Mapping_Edited = "Mapping Edited";
        public const string Permission_Group_Deleted = "Group Deleted";
        public const string Permission_Level_Deleted = "Level Deleted";
        public const string Permission_Mapping_Deleted = "Mapping Deleted";

    }
    public class AuditLogger
    {


        public SystemAudit System;
        public UserAudit User;
        public GroupAudit Group;
        public ComputerAudit Computer;
        public OUAudit OU;
        public LogonAudit Logon;

        public AuditLogger(IAppDatabaseFactory factory, IApplicationUserStateService userStateService)
        {
            System = new SystemAudit(factory);
            User = new UserAudit(factory, userStateService);
            Group = new GroupAudit(factory, userStateService);
            Computer = new ComputerAudit(factory, userStateService);
            OU = new OUAudit(factory, userStateService);
            Logon = new LogonAudit(factory, userStateService);
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
        }

    }

    public class OUAudit : DirectoryAudit
    {
        public OUAudit(IAppDatabaseFactory factory,
            IApplicationUserStateService userStateService) : base(factory, userStateService)
        {
        }

        public override async Task<bool> Deleted(IDirectoryEntryAdapter deletedEntry)
         => await Log(t => t.DirectoryEntryAuditLogs,
             AuditActions.OU_Deleted, deletedEntry);


        public override async Task<bool> Searched(IDirectoryEntryAdapter searchedOU)
            => await Log<DirectoryEntryAuditLog>(c => c.DirectoryEntryAuditLogs,
                AuditActions.Group_Searched,
                searchedOU);

        public override async Task<bool> Created(IDirectoryEntryAdapter newOU)

        {
            var oldValues = "";
            var newValues = "";
            foreach (var c in newOU.NewEntryProperties)
            {
                newValues += c.Key + "=" + c.Value;
            }
            await Log<DirectoryEntryAuditLog>(c => c.DirectoryEntryAuditLogs, AuditActions.OU_Created, newOU, oldValues, newValues);
            return true;
        }

    }

    public class ComputerAudit : DirectoryAudit
    {
        public ComputerAudit(IAppDatabaseFactory factory,
            IApplicationUserStateService userStateService) : base(factory, userStateService)
        {
        }


        public override async Task<bool> Deleted(IDirectoryEntryAdapter deletedEntry)
         => await Log(t => t.DirectoryEntryAuditLogs,
             AuditActions.Computer_Deleted, deletedEntry);

        public async Task<bool> Searched(IADComputer searchedComputer) => await Log(AuditActions.Computer_Searched, searchedComputer);

        private async Task<bool> Log(string action, IADComputer searchedComputer)
        {

            try
            {
                using var context = await Factory.CreateDbContextAsync();
                context.DirectoryEntryAuditLogs.Add(new ComputerAuditLog
                {
                    Action = action,
                    Target = searchedComputer.CanonicalName,
                    Username = UserStateService?.CurrentUsername

                });
                context.SaveChanges();
                return true;
            }
            catch(Exception ex)
            {
                Loggers.SystemLogger.Error("Unable to write Log to database", ex);

                return false;
            }
        }
    }

    public class GroupAudit : DirectoryAudit
    {
        public GroupAudit(IAppDatabaseFactory factory,
            IApplicationUserStateService userStateService) : base(factory, userStateService)
        {
        }
        public override async Task<bool> Deleted(IDirectoryEntryAdapter deletedEntry)
         => await Log(t => t.DirectoryEntryAuditLogs,
             AuditActions.Group_Deleted, deletedEntry);
        public override async Task<bool> Searched(IDirectoryEntryAdapter searchedGroup) => await Log<DirectoryEntryAuditLog>(c => c.DirectoryEntryAuditLogs, AuditActions.Group_Searched, searchedGroup);

        public async Task<bool> Assigned (IDirectoryEntryAdapter member, IDirectoryEntryAdapter parent)
        {
            await Log<DirectoryEntryAuditLog>(c => c.DirectoryEntryAuditLogs,
               AuditActions.Group_Assigned,
            member,
               null,
               "Assigned to" + parent.DN);
            await Log<DirectoryEntryAuditLog>(c => c.DirectoryEntryAuditLogs,
              AuditActions.Group_Assigned,
           parent,
              null,
              "Added member "+member.DN);
            return true;
        }
        public override async Task<bool> Changed(IDirectoryEntryAdapter changedGroup, List<AuditChangeLog> changes)
        {

            await Log<DirectoryEntryAuditLog>(c => c.DirectoryEntryAuditLogs,
                AuditActions.Group_Edited,
                changedGroup,
                changes.GetValueChangesString(c => c.OldValue),
                changes.GetValueChangesString(c => c.NewValue));
            return true;
        }
        public override async Task<bool> Created(IDirectoryEntryAdapter newGroup)
        {
            var oldValues = "";
            var newValues = "";
            foreach (var c in newGroup.NewEntryProperties)
            {
                newValues += c.Key + "=" + c.Value;
            }
            await Log<DirectoryEntryAuditLog>(c => c.DirectoryEntryAuditLogs,
                AuditActions.Group_Created,
                newGroup,
                oldValues,
                newValues);
            return true;
        }
    }
    public class LogonAudit : CommonAudit
    {
        public LogonAudit(IAppDatabaseFactory factory,
            IApplicationUserStateService userStateService) : base(factory, userStateService)
        {
        }

        public async Task<bool> Login(System.Security.Claims.ClaimsPrincipal user)
        {
            CurrentUser = UserStateService.CreateUserState(user);
            return await Log("Login");
        }
        public async Task<bool> Logout() => await Log("Logout");

        private async Task<bool> Log(string action)
        {

            try
            {
                using var context = await Factory.CreateDbContextAsync();
                context.LogonAuditLog.Add(new LogonAuditLog
                {
                    Action = action,
                    Username = CurrentUser.AuditUsername,
                    IpAddress = CurrentUser.IPAddress?.ToString(),
                }) ;
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class UserAudit : DirectoryAudit
    {
        public UserAudit(IAppDatabaseFactory factory, IApplicationUserStateService userStateService) : base(factory, userStateService)
        {
        }

        public override async Task<bool> Deleted(IDirectoryEntryAdapter deletedEntry)
            => await Log(t => t.DirectoryEntryAuditLogs,
                AuditActions.User_Deleted, deletedEntry);

        public override async Task<bool> Searched(IDirectoryEntryAdapter searchedUser)
            => await Log<DirectoryEntryAuditLog>(c => c.DirectoryEntryAuditLogs,
                AuditActions.User_Searched,
                searchedUser);

        public async Task<bool> PasswordChanged(IDirectoryEntryAdapter searchedUser,
            bool requirePasswordChanged = false)
            => await Log<DirectoryEntryAuditLog>(c => c.DirectoryEntryAuditLogs, AuditActions.Password_Changed, searchedUser, null, "requirePasswordChange=" + requirePasswordChanged);

        public async Task<bool> Assigned(IDirectoryEntryAdapter member, IDirectoryEntryAdapter parent)
        {
            await Log<DirectoryEntryAuditLog>(c => c.DirectoryEntryAuditLogs,
               AuditActions.User_Assigned,
            member,
               null,
               "Assigned to" + parent.DN);
            await Log<DirectoryEntryAuditLog>(c => c.DirectoryEntryAuditLogs,
              AuditActions.User_Assigned,
           parent,
              null,
              "Added member " + member.DN);
            return true;
        }

        public override async Task<bool> Created(IDirectoryEntryAdapter newUser)
        {
            var oldValues = "";
            var newValues = "";
            foreach (var c in newUser.NewEntryProperties)
            {
                newValues += c.Key + "=" + c.Value;
            }
            await Log<DirectoryEntryAuditLog>(c => c.DirectoryEntryAuditLogs, AuditActions.User_Created, newUser, oldValues, newValues);
            return true;
        }

        public override async Task<bool> Changed(IDirectoryEntryAdapter changedUser, List<AuditChangeLog> changes)
        {
            await Log<DirectoryEntryAuditLog>(c => c.DirectoryEntryAuditLogs, AuditActions.User_Edited, changedUser, changes.GetValueChangesString(c => c.OldValue), changes.GetValueChangesString(c => c.NewValue));
            return true;
        }



    }
    public class CommonAudit : BaseAudit
    {
        protected IApplicationUserStateService UserStateService { get; private set; }
        protected IApplicationUserState? CurrentUser { get; set; }
        public CommonAudit(IAppDatabaseFactory factory, IApplicationUserStateService userStateService) : base(factory)
        {
            UserStateService = userStateService;
            CurrentUser = UserStateService.CurrentUserState;
            
        }
    }
    public class DirectoryAudit : CommonAudit
    {
        public DirectoryAudit(IAppDatabaseFactory factory, IApplicationUserStateService userStateService) : base(factory, userStateService)
        {
        }

        public virtual Task<bool> Changed(IDirectoryEntryAdapter changedUser, List<AuditChangeLog> changes)
        {
            throw new NotImplementedException();
        }
        public virtual async Task<bool> Deleted(IDirectoryEntryAdapter deletedEntry)
        {
            throw new NotImplementedException();

        }
        public virtual Task<bool> Created(IDirectoryEntryAdapter newUser)
        {
            throw new NotImplementedException();
        }

        public virtual Task<bool> Searched(IDirectoryEntryAdapter searchedUser)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">The Log Entry Type to insert</typeparam>
        /// <param name="auditTable"></param>
        /// <param name="action"></param>
        /// <param name="relatedEntry"></param>
        /// <param name="beforeAction"></param>
        /// <param name="afterAction"></param>
        /// <returns></returns>
        protected virtual async Task<bool> Log<T>(
            Func<IDatabaseContext,
            DbSet<T>> auditTable,
            string action,
            IDirectoryEntryAdapter relatedEntry,
            string? beforeAction = null,
            string? afterAction = null) where T : class, IDirectoryEntryAuditLog, new()
        {

            try
            {
                using var context = await Factory.CreateDbContextAsync();
                var table = auditTable.Invoke(context);
                var auditEntry = new T()
                {
                    Action = action,
                    Target = relatedEntry.CanonicalName,
                    Sid = relatedEntry.SID.ToSidString(),
                    BeforeAction = beforeAction,
                    AfterAction = afterAction,
                    Username = CurrentUser.AuditUsername,
                    IpAddress = CurrentUser.IPAddress?.ToString(),
                };
                table.Add(auditEntry);
                context.SaveChanges();
                return true;

            }
            catch
            {
                return false;
            }
        }
    }

    public class SystemAudit : BaseAudit
    {

        public SystemAudit(IAppDatabaseFactory factory) : base(factory)
        {
        }
        public async Task<bool> LogMessage(string message)
        {

            return await Log(message);
        }

        public async Task<bool> SettingsChanged(string category, List<AuditChangeLog> changes)
        {

            return await Log("Settings_Changed",
                changes.GetValueChangesString(c => c.OldValue),
                changes.GetValueChangesString(c => c.NewValue)
                );
        }


        private async Task<bool> Log(string action,
            string? beforeAction = null,
            string? afterAction = null)
        {
            try
            {
                using var context = await Factory.CreateDbContextAsync();
                context.SystemAuditLog.Add(new SystemAuditLog
                {
                    Action = action,
                    Username = "System",
                    BeforeAction = beforeAction,
                    AfterAction = afterAction,
                    Timestamp = DateTime.Now,



                });
                context.SaveChanges();
                return true;

            }
            catch
            {
                return false;
            }
        }
    }

    public class BaseAudit
    {
        protected IAppDatabaseFactory Factory { get; set; }

        public BaseAudit(IAppDatabaseFactory factory)
        {
            Factory = factory;
        }
    }
}