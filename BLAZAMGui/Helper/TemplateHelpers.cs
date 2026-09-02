using BLAZAM.Global.Data;
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
        public static async Task<IADUser> GenerateTemplateUserAsync(this DirectoryTemplate template, NewUserName newUserName, IActiveDirectoryContext directory, IADOrganizationalUnit? parentOU = null)
        {
            Loggers.ActiveDirectoryLogger.Information("Generating user from template {@TemplateName} for {@NewUserName}", template.Name, newUserName);
            IADUser? newUser;
            if (parentOU == null)
            {
                Loggers.ActiveDirectoryLogger.Debug("No parent OU specified, searching for template effective parent OU {@EffectiveParentOU}", template.EffectiveParentOU);
                parentOU = (await directory.OUs.FindOuByDNAsync(template.EffectiveParentOU));
            }
            if (parentOU == null)
            {
                throw new AppException("OU could not be found for new user");
            }

            Loggers.ActiveDirectoryLogger.Debug("Using parent OU {@ParentOU} for new user creation", parentOU.DN);
            var displayName = template.GenerateDisplayName(newUserName);
            Loggers.ActiveDirectoryLogger.Debug("Generated display name {@DisplayName} for new user", displayName);
            try
            {
                Loggers.ActiveDirectoryLogger.Information("Creating user {@DisplayName} in {@ContainerName}", displayName, parentOU.DN);
                newUser = parentOU.CreateUser(displayName);

                newUser.SAMAccountName = template.GenerateUsername(newUserName);
                if (template.HasIncrementorVariable)
                {
                    var conflictAttempt = 0;
                    while(directory.Users.FindUserByUsername(newUser.SAMAccountName,exactMatch:true) != null)
                    {
                        newUser.SAMAccountName = template.GenerateUsername(newUserName, conflictAttempt + 2);
                        conflictAttempt++;
                        if (conflictAttempt > 28)
                        {
                            throw new AppException("Could not generate a unique username after multiple attempts. Please adjust the template or try again.");
                        }
                    }
                }
                newUser.DisplayName = displayName;
                newUser.StagePasswordChange(template.GeneratePassword(newUserName).ToSecureString());
                newUser.StageEnable();
                if (template.EffectiveRequirePasswordChange == true)
                {
                    newUser.StageRequirePasswordChange(true);
                }

                if (!newUserName.GivenName.IsNullOrEmpty())
                {
                    newUser.GivenName = newUserName.GivenName;
                }

                if (!newUserName.MiddleName.IsNullOrEmpty())
                {
                    newUser.MiddleName = newUserName.MiddleName;
                }

                if (!newUserName.Surname.IsNullOrEmpty())
                {
                    newUser.Sn = newUserName.Surname;
                }

                template.EffectiveAssignedGroupSids.ForEach(sid =>
                {
                    var group = directory.Groups.FindGroupBySID(sid.GroupSid);
                    if (group != null)
                    {
                        newUser.AssignTo(group);
                    }
                });
                Loggers.ActiveDirectoryLogger.Information("User {@DisplayName} staged successfully in {@ContainerName}", displayName, parentOU.DN);
                return newUser;
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Error(ex, "Error while attempting to create user in {@ContainerName}", parentOU.DN);

                throw;
            }
          
        }
    }
}
