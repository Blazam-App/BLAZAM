using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.ActiveDirectory.Models;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Models.Database.Audit;
using Microsoft.EntityFrameworkCore;

namespace BLAZAM.Server.Data.Services
{
    public class AuditActions
    {

        //User Actions
        public static string User_Searched = "User Searched";
        public static string User_Enabled = "User Enabled";
        public static string User_Disabled = "User Disabled";
        public static string User_Assigned = "User Assigned";
        public static string User_Unassigned = "User Unassigned";
        public static string User_Unlocked = "User Unlocked";
        public static string User_Created = "User Created";
        public static string User_Deleted = "User Deleted";
        public static string User_Moved = "User Moved";
        public static string User_Edited = "User Edited";

        //Computer Actions
        public static string Computer_Searched = "Computer Searched";
        public static string Computer_Enabled = "Computer Enabled";
        public static string Computer_Disabled = "Computer Disabled";
        public static string Computer_Assigned = "Computer Assigned";
        public static string Computer_Unassigned = "Computer Unassigned";
        public static string Computer_Unlocked = "Computer Unlocked";
        public static string Computer_Created = "Computer Created";
        public static string Computer_Deleted = "Computer Deleted";
        public static string Computer_Moved = "Computer Moved";
        public static string Computer_Edited = "Computer Edited";


        //Group Actions
        public static string Group_Searched = "Group Searched";
        public static string Group_Assigned = "Group Assigned";
        public static string Group_Unassigned = "Group Unassigned";
        public static string Group_Created = "Group Created";
        public static string Group_Deleted = "Group Deleted";
        public static string Group_Moved = "Group Moved";
        public static string Group_Edited = "Group Edited";


        //OU Actions
        public static string OU_Searched = "OU Searched";
        public static string OU_Created = "OU Created";
        public static string OU_Deleted = "OU Deleted";
        public static string OU_Moved = "OU Moved";
        public static string OU_Edited = "OU Edited";

        //Settings Actions
        public static string Settings_Edited = "Settings Edited";

        //Permissions Actions
        public static string Permission_Group_Added = "Group Added";
        public static string Permission_Level_Added = "Level Added";
        public static string Permission_Mapping_Added = "Mapping Added";
        public static string Permission_Group_Edited = "Group Edited";
        public static string Permission_Level_Edited = "Level Edited";
        public static string Permission_Mapping_Edited = "Mapping Edited";
        public static string Permission_Group_Deleted = "Group Deleted";
        public static string Permission_Level_Deleted = "Level Deleted";
        public static string Permission_Mapping_Deleted = "Mapping Deleted";

    }
    public class AuditLogger : IDisposable
    {


        public SystemAudit System;
        public UserAudit User;
        public GroupAudit Group;
        public ComputerAudit Computer;
        public OUAudit OU;
        public LogonAudit Logon;

        public AuditLogger(IDbContextFactory<DatabaseContext> factory, IApplicationUserStateService userStateService)
        {
            System = new SystemAudit(factory);
            User = new UserAudit(factory, userStateService);
            Group = new GroupAudit(factory, userStateService);
            Computer = new ComputerAudit(factory, userStateService);
            OU = new OUAudit(factory, userStateService);
            Logon = new LogonAudit(factory, userStateService);
        }

        public void Dispose()
        {
            System = null;
            User = null;
            Group = null;
            Computer = null;
            Logon = null;
            OU = null;
        }
    }

    public class OUAudit : CommonAudit
    {
        public OUAudit(IDbContextFactory<DatabaseContext> factory, IApplicationUserStateService userStateService) : base(factory, userStateService)
        {
        }
        public async Task<bool> Searched(IADOrganizationalUnit searchedOU) => await Log(AuditActions.Group_Searched, searchedOU);

        private async Task<bool> Log(string action, IADOrganizationalUnit searchedOU)
        {

            try
            {
                using (var context = await Factory.CreateDbContextAsync())
                {
                    context.OUAuditLog.Add(new OUAuditLog
                    {
                        Action = action,
                        Target = searchedOU.OU

                    });
                    context.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    public class ComputerAudit : CommonAudit
    {
        public ComputerAudit(IDbContextFactory<DatabaseContext> factory, IApplicationUserStateService userStateService) : base(factory, userStateService)
        {
        }
        public async Task<bool> Searched(IADComputer searchedComputer) => await Log(AuditActions.Computer_Searched, searchedComputer);

        private async Task<bool> Log(string action, IADComputer searchedComputer)
        {

            try
            {
                using (var context = await Factory.CreateDbContextAsync())
                {
                    context.ComputerAuditLog.Add(new ComputerAuditLog
                    {
                        Action = action,
                        Target = searchedComputer.CanonicalName,
                        Username = UserStateService?.CurrentUsername

                    });
                    context.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    public class GroupAudit : CommonAudit
    {
        public GroupAudit(IDbContextFactory<DatabaseContext> factory, IApplicationUserStateService userStateService) : base(factory, userStateService)
        {
        }
        public async Task<bool> Searched(IADGroup searchedGroup) => await Log(AuditActions.Group_Searched, searchedGroup);

        public async Task<bool> Changed(IADGroup changedGroup, List<DirectoryModelChange> changes)
        {
            var oldValues = "";
            var newValues = "";
            foreach (var c in changes)
            {
                oldValues += c.Field + "=" + c.OldValue;
                newValues += c.Field + "=" + c.NewValue;
            }
            await Log("Group Changed", changedGroup, oldValues, newValues);
            return true;
        }

        private async Task<bool> Log(string action, IADGroup searchedGroup, string? beforeAction = null, string? afterAction = null)
        {

            try
            {
                using (var context = await Factory.CreateDbContextAsync())
                {
                    context.GroupAuditLog.Add(new GroupAuditLog
                    {
                        Action = action,
                        Target = searchedGroup.GroupName,
                        BeforeAction=beforeAction,
                        AfterAction = afterAction,
                        Username = CurrentUser.AuditUsername
                    });
                    context.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
    public class LogonAudit : CommonAudit
    {
        public LogonAudit(IDbContextFactory<DatabaseContext> factory, IApplicationUserStateService userStateService) : base(factory, userStateService)
        {
        }

        public async Task<bool> Login(System.Security.Claims.ClaimsPrincipal user)
        {
            CurrentUser = new ApplicationUserState() { User=user};
            return await Log("Login");
        }
        public async Task<bool> Logout() => await Log("Logout");

        private async Task<bool> Log(string action)
        {

            try
            {
                using (var context = await Factory.CreateDbContextAsync())
                {
                    context.LogonAuditLog.Add(new LogonAuditLog
                    {
                        Action = action,
                        Username = CurrentUser.AuditUsername
                    });
                    context.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    public class UserAudit : CommonAudit
    {
        public UserAudit(IDbContextFactory<DatabaseContext> factory, IApplicationUserStateService userStateService) : base(factory, userStateService)
        {
        }

        public async Task<bool> Searched(IADUser searchedUser) => await Log(AuditActions.User_Searched, searchedUser);

        public async Task<bool> Changed(IADUser changedUser,List<DirectoryModelChange> changes)
        {
            var oldValues = "";
            var newValues = "";
            foreach (var c in changes)
            {
                oldValues += c.Field + "=" + c.OldValue;
                newValues += c.Field + "=" + c.NewValue;
            }
            await Log("User Changed",changedUser,oldValues, newValues);
            return true;
        }

        private async Task<bool> Log(string action, IADUser user,string? beforeAction = null, string? afterAction = null)
        {

            try
            {
                using (var context = await Factory.CreateDbContextAsync())
                {

                    context.UserAuditLog.Add(new UserAuditLog
                    {
                        Action = action,
                        Target = user.SamAccountName,
                        Username = CurrentUser.AuditUsername,
                        BeforeAction = beforeAction,
                        AfterAction = afterAction
                    });
                    context.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

    }

    public class CommonAudit : BaseAudit
    {
        public CommonAudit(IDbContextFactory<DatabaseContext> factory, IApplicationUserStateService userStateService) : base(factory)
        {
            UserStateService = userStateService;
            CurrentUser = UserStateService.CurrentUserState;

        }

        protected IApplicationUserStateService UserStateService { get; private set; }
        protected IApplicationUserState? CurrentUser { get; set; }
    }

    public class SystemAudit : BaseAudit
    {
         
        public SystemAudit(IDbContextFactory<DatabaseContext> factory) : base(factory)
        {
        }
        public async Task<bool> LogMessage(string message)
        {
            
           return await Log(message);
        }



        private async Task<bool> Log(string action)
        {
            try
            {
                using (var context = await Factory.CreateDbContextAsync())
                {
                    context.SystemAuditLog.Add(new SystemAuditLog
                    {
                        Action = action,
                        Username = "System",
                         Timestamp= DateTime.Now,



                    }) ;
                    context.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    public class BaseAudit
    {
        protected IDbContextFactory<DatabaseContext> Factory { get; set; }

        public BaseAudit(IDbContextFactory<DatabaseContext> factory)
        {
            Factory = factory;
        }
    }
}