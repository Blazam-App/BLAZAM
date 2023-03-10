using BLAZAM.Common.Data.ActiveDirectory;
using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Models.Database.Permissions;
using Microsoft.EntityFrameworkCore;
namespace BLAZAM.Server.Data.Services
{
    public class PermissionHandler
    {
        protected IDbContextFactory<DatabaseContext> Factory { get; set; }
        protected IActiveDirectory Directory { get; set; }

        public PermissionHandler(IDbContextFactory<DatabaseContext> factory, IActiveDirectory directory)
        {
            Factory = factory;
            Directory = directory;
            directory.OnNewLoginUser += LoadPermissionsForNewLoginUser;
        }

        private async void LoadPermissionsForNewLoginUser(IApplicationUserState value)
        {
            if(value.DirectoryUser!=null)
            await LoadPermissions(value.DirectoryUser);

        }


        internal async Task LoadPermissions(IADUser user)
        {
            using (var Context = await Factory.CreateDbContextAsync())
            {
                var cursor = await Context.PermissionDelegate.Include(pl=>pl.PermissionsMaps).ToListAsync();
                foreach(var l in cursor) { 
                
                    
                    
                    if (user.IsAMemberOf(ActiveDirectoryContext.Instance.Groups.FindGroupBySID(l.DelegateSid)))
                    {
                        user.PermissionDelegates.Add(l);
                        user.PermissionMappings.AddRange(l.PermissionsMaps);
                    }
                }
               
            }
        }
    }
}
