using Microsoft.Win32.SafeHandles;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
namespace BLAZAM.Common.Data
{
    /// <summary>
    /// Provides a way to run code under other identities
    /// </summary>
    public class WindowsImpersonation
    {
        private SafeAccessTokenHandle safeAccessTokenHandle;
        private WindowsImpersonationUser? impersonationUser;
        private readonly WindowsIdentity ApplicationIdentity;

        private SafeAccessTokenHandle GetImpersonatedToken()
        {
            if (impersonationUser == null)
            {
                throw new AppException("Attempted to impersonate without an impersonation user");
            }
            //Use interactive logon
            var domain = impersonationUser.FQDN ?? "";
            var username = impersonationUser.Username;
            var phPassword = Marshal.SecureStringToGlobalAllocUnicode(impersonationUser.Password);
            bool returnValue = LogonUser(username,
                    domain,
                    phPassword,
                    LOGON32_LOGON_NETWORK,
                    LOGON32_PROVIDER_DEFAULT,
                    out safeAccessTokenHandle);



            Marshal.ZeroFreeGlobalAllocUnicode(phPassword);
            if (!returnValue)
            {
                int ret = Marshal.GetLastWin32Error();
                Loggers.ActiveDirectoryLogger.Warning("LogonUser failed with error code : {0}", ret);
                var exception = new System.ComponentModel.Win32Exception(ret);


                throw new AuthenticationException(exception.Message);

            }
            return safeAccessTokenHandle;

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
        private static extern bool CloseHandle(IntPtr handle);
        /// <summary>
        /// Creates a new impersonation context under the provided <see cref="WindowsImpersonationUser"/>
        /// </summary>
        /// <param name="user"></param>
        public WindowsImpersonation(WindowsImpersonationUser? user)
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
        public async Task<T?> RunAsync<T>(Func<T> task) => await Task.Run(() => Run(task));

        /// <summary>
        /// Runs the provided async action as the <see cref="WindowsImpersonationUser"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        public async Task<T?> RunAsync<T>(Func<Task<T>> task) =>
            await Task.Run(() => RunImpersonated(() => task().GetAwaiter().GetResult()));

        /// <summary>
        /// Runs the provided action synchronously as the <see cref="WindowsImpersonationUser"/> or as the application identity if no user was provided
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        public T? Run<T>(Func<T> task) => RunImpersonated(() => task());

        /// <summary>
        /// Core impersonation logic shared by all Run methods
        /// </summary>
        private T? RunImpersonated<T>(Func<T> executeTask)
        {
            // If no impersonation user provided, run as application identity
            if (impersonationUser == null)
            {
                Loggers.ActiveDirectoryLogger.Information("Running as application identity: {@Identity}", WindowsIdentity.GetCurrent().Name);
                return executeTask();
            }

            T? result = default;
            try
            {
                var impersonatedToken = GetImpersonatedToken();

                if (impersonatedToken == null)
                {
                    throw new AppException("The impersonation user is invalid. Check settings.");
                }

                // Check the identity.
                Loggers.ActiveDirectoryLogger.Information("Before impersonation: {@PreIdentity}", WindowsIdentity.GetCurrent().Name);

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
                          Loggers.ActiveDirectoryLogger.Information("During impersonation: {@PostIdentity}", WindowsIdentity.GetCurrent().Name);
                          result = executeTask();
                      }
                      );
                }
                catch (IdentityNotMappedException ex)
                {
                    Loggers.ActiveDirectoryLogger.Information(ex, "The identity could not be mapped to a Windows account {@Impersonatee}", impersonationUser.Username);
                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectoryLogger.Error(ex, "Error running impersonated action {@Impersonatee}", impersonationUser.Username);
                }
                finally
                {
                    impersonatedToken.Close();
                }
            }
            catch (AuthenticationException ex)
            {
                Loggers.ActiveDirectoryLogger.Information(ex, "Bad credentials trying to impersonate user {@Username}", impersonationUser.Username);
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Information(ex, "Error trying to impersonate {@Impersonatee}", impersonationUser.Username);
            }

            return result;
        }


    }
}