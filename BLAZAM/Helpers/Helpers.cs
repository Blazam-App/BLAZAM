using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Models.Templates;
using BLAZAM.EmailMessage.Email.Notifications;
using MudBlazor;
using System.Security;

namespace BLAZAM.Helpers
{
    public static class Helpers
    {
        public static IADUser GenerateTemplateUser(this DirectoryTemplate template, NewUserName newUserName, IActiveDirectoryContext directory)
        {
            IADUser? newUser;
            var ou = directory.OUs.FindOuByString(template.EffectiveParentOU).FirstOrDefault();
            if (ou == null) throw new ApplicationException("OU could not be found for new user");
            var displayName = template.GenerateDisplayName(newUserName);
            newUser = ou.CreateUser(displayName);

            newUser.SamAccountName = template.GenerateUsername(newUserName);
            newUser.DisplayName = displayName;
            //newUser.SetPassword(template.GeneratePassword().ToSecureString(),false);
            //newUser.CanonicalName = template.GenerateDisplayName(newUserName);
            newUser.StagePasswordChange(template.GeneratePassword(newUserName).ToSecureString());
            if (template.EffectiveRequirePasswordChange == true)
                newUser.StageRequirePasswordChange(true);
            if (!newUserName.GivenName.IsNullOrEmpty())
                newUser.GivenName = newUserName.GivenName;
            if (!newUserName.MiddleName.IsNullOrEmpty())
                newUser.MiddleName = newUserName.MiddleName;
            if (!newUserName.Surname.IsNullOrEmpty())
                newUser.Surname = newUserName.Surname;



            template.EffectiveAssignedGroupSids.ForEach(sid =>
            {
                var group = directory.Groups.FindGroupBySID(sid.GroupSid);
                if (group != null)
                    newUser.AssignTo(group);

            });
            return newUser;
        }
        
    }
}
