using BLAZAM.Common.Data;
using BLAZAM.Database.Models;


namespace BLAZAM.Helpers
{
    public static class WindowsHelpers
    {
        public static WindowsImpersonationUser GetDirectoryImpersonationUser(this ADSettings settings)
        {
            
                return new WindowsImpersonationUser()
                {
                    FQDN = settings.FQDN,
                    Username = settings.Username,

                    Password = settings.Password.Decrypt().ToSecureString(),
                };
            
        }
        public static WindowsImpersonationUser GetUpdateImpersonationUser(this AppSettings settings)
        {

            return new WindowsImpersonationUser()
            {
                FQDN = settings.UpdateDomain,
                Username = settings.UpdateUsername,
                Password = settings.UpdatePassword.Decrypt().ToSecureString()
            };

        }

        /// <summary>
        /// Creates a windows identity from the active directory
        /// user defined in settings
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static WindowsImpersonation CreateDirectoryAdminImpersonator(this ADSettings settings)
        {
            return new(settings.GetDirectoryImpersonationUser());
        }
        /// <summary>
        /// Creates a windows identity from the update settins
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static WindowsImpersonation? CreateUpdateImpersonator(this AppSettings settings)
        {
            if (settings != null && settings.UpdateUsername != null && settings.UpdatePassword != null)
                return new(settings.GetUpdateImpersonationUser());
            else
                return null;
        }
    }
}
