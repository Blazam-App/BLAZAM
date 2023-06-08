using BLAZAM.Common.Data;
using BLAZAM.Database.Models;


namespace BLAZAM.Helpers
{
    public static class WindowsHelpers
    {
        /// <summary>
        /// Creates a windows identity from the active directory
        /// user defined in settings
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static WindowsImpersonation CreateWindowsImpersonator(this ADSettings settings)
        {
            return new(new()
            {
                FQDN = settings.FQDN,
                Username = settings.Username,

                Password =settings.Password.Decrypt().ToSecureString(),
            });
        }
        /// <summary>
        /// Creates a windows identity from the update settins
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static WindowsImpersonation CreateWindowsImpersonator(this AppSettings settings)
        {
            return new(new()
            {
                FQDN = settings.UpdateDomain,
                Username = settings.UpdateUsername,
                Password = settings.UpdatePassword.Decrypt().ToSecureString()
            });
        }
    }
}
