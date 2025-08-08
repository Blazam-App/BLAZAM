using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;
namespace BLAZAM.Common.Data
{
    /// <summary>
    /// Provides a way to run code under other identities
    /// </summary>
    public class WindowsImpersonation
    {
        private SafeAccessTokenHandle safeAccessTokenHandle;
        private WindowsImpersonationUser impersonationUser;
        private readonly WindowsIdentity ApplicationIdentity;

        private SafeAccessTokenHandle ImpersonatedToken
        {
            get
            {
                //Use interactive logon
                var domain = impersonationUser.FQDN ?? "";
                var username = impersonationUser.Username;
                var phPassword = Marshal.SecureStringToGlobalAllocUnicode(impersonationUser.Password);
                bool returnValue = LogonUser(username,
                        domain,
                        phPassword,
                        LOGON32_LOGON_INTERACTIVE,
                        LOGON32_PROVIDER_DEFAULT,
                        out safeAccessTokenHandle);



                Marshal.ZeroFreeGlobalAllocUnicode(phPassword);
                if (false == returnValue)
                {
                    int ret = Marshal.GetLastWin32Error();
                    Loggers.ActiveDirectoryLogger.Warning("LogonUser failed with error code : {0}", ret);
                    var exception = new System.ComponentModel.Win32Exception(ret);
                    if (exception.NativeErrorCode == 1326)
                    {

                        throw new AuthenticationException(exception.Message);
                    }
                }
                return safeAccessTokenHandle;

            }
        }

        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_LOGON_NETWORK = 3;
        private const int LOGON32_LOGON_BATCH = 4;
        private const int LOGON32_LOGON_SERVICE = 5;
        private const int LOGON32_LOGON_UNLOCK = 7;
        private const int LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
        private const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON32_PROVIDER_WINNT50 = 3;
        //This parameter causes LogonUser to create a primary token. 



        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, IntPtr lpszPassword,
    int dwLogonType, int dwLogonProvider, out SafeAccessTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private extern static bool CloseHandle(IntPtr handle);
        /// <summary>
        /// Creates a new impersonation context under the provided <see cref="WindowsImpersonationUser"/>
        /// </summary>
        /// <param name="user"></param>
        public WindowsImpersonation(WindowsImpersonationUser user)
        {
            impersonationUser = user;
            ApplicationIdentity = WindowsIdentity.GetCurrent();
        }
        /// <summary>
        /// Runs the provided action asynchronously as the <see cref="WindowsImpersonationUser"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        public async Task<T?> RunAsync<T>(Func<T> task) => await Task.Run(() => Run<T>(task));
        /// <summary>
        /// Runs the provided action synchronously as the <see cref="WindowsImpersonationUser"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public T? Run<T>(Func<T> task)
        {


            T? result = default;
            try
            {
                var impersonatedToken = ImpersonatedToken;


                if (impersonatedToken == null) throw new AppException("The impersonation user is invalid. Check settings.");


                // Check the identity.
                Loggers.ActiveDirectoryLogger.Information("Before impersonation: " + WindowsIdentity.GetCurrent().Name);

                try
                {

                    WindowsIdentity.RunImpersonated(
                      impersonatedToken,
                      () =>
                      {
                          // Check the identity.
                          var impersonatedIdentity = WindowsIdentity.GetCurrent();
                          if (impersonationUser.Username != ApplicationIdentity.Name && impersonatedIdentity.Name.Equals(ApplicationIdentity.Name))
                          {
                              var exception = new AppException("Impersonation running as application identity");
                              ExceptionDispatchInfo.SetCurrentStackTrace(exception);
                              Loggers.ActiveDirectoryLogger.Information(exception, "Impersonation running as application identity");

                          }
                          Loggers.ActiveDirectoryLogger.Information("During impersonation: " + WindowsIdentity.GetCurrent().Name);
                          result = task.Invoke();
                      }
                      );

                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectoryLogger.Error(ex, "Error running impersonated action {@Impersonatee}", impersonationUser.Username);
                }
                finally
                {
                    impersonatedToken?.Close();
                }
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Error(ex, "Error trying to impersonate {@Impersonatee}", impersonationUser.Username);
            }

            return result;
        }


    }
}