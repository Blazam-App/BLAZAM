using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.Gui.Helpers
{
    public static class TemplateHelpers
    {
        /// <summary>
        /// Processes a template given a <see cref="NewUserName"/>
        /// </summary>
        /// <param name="template">The template to be applied</param>
        /// <param name="newUserName">The new user name</param>
        /// <param name="directory">The directory to create the use under</param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public static IADUser GenerateTemplateUser(this DirectoryTemplate template, NewUserName newUserName, IActiveDirectoryContext directory)
        {
            IADUser? newUser;
            var ou = directory.OUs.FindOuByString(template.EffectiveParentOU).FirstOrDefault();
            if (ou == null) throw new AppException("OU could not be found for new user");
            var displayName = template.GenerateDisplayName(newUserName);
            newUser = ou.CreateUser(displayName);

            newUser.SamAccountName = template.GenerateUsername(newUserName);
            newUser.DisplayName = displayName;
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
