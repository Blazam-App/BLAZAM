using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using BLAZAM.Common.Exceptions;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using Microsoft.Win32.SafeHandles;
using Serilog;
namespace BLAZAM.Common.Data
{
    public class WindowsImpersonation
    {
        SafeAccessTokenHandle safeAccessTokenHandle;

        WindowsImpersonationUser impersonationUser;
        private readonly WindowsIdentity ApplicationIdentity;

        public SafeAccessTokenHandle ImpersonatedToken
        {
            get
            {
                //Use interactive logon
                var domain = impersonationUser.FQDN != null ? impersonationUser.FQDN : "";
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
                    Loggers.ActiveDirectryLogger.Warning("LogonUser failed with error code : {0}", ret);
                    var exception = new System.ComponentModel.Win32Exception(ret);
                    if (exception.NativeErrorCode == 1326)
                    {

                        throw new AuthenticationException(exception.Message);
                    }
                }
                return safeAccessTokenHandle;

            }
        }


        const int LOGON32_LOGON_INTERACTIVE = 2;
        const int LOGON32_LOGON_NETWORK = 3;
        const int LOGON32_LOGON_BATCH = 4;
        const int LOGON32_LOGON_SERVICE = 5;
        const int LOGON32_LOGON_UNLOCK = 7;
        const int LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
        const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_PROVIDER_WINNT50 = 3;
        //This parameter causes LogonUser to create a primary token. 



        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(string lpszUsername, string lpszDomain, IntPtr lpszPassword,
    int dwLogonType, int dwLogonProvider, out SafeAccessTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        public WindowsImpersonation(WindowsImpersonationUser user)
        {
            impersonationUser = user;
            ApplicationIdentity = WindowsIdentity.GetCurrent();
        }
        public async Task<T?> RunAsync<T>(Func<T> task) => await Task.Run(() => Run<T>(task));
        public T? Run<T>(Func<T> task)
        {


            T? result = default;
            try
            {
                var impersonatedToken = ImpersonatedToken;


                if (impersonatedToken == null) throw new ApplicationException("The impersonation user is invalid. Check settings.");

                //Console.WriteLine("Did LogonUser Succeed? " + (returnValue ? "Yes" : "No"));
                // Check the identity.
                Loggers.ActiveDirectryLogger.Information("Before impersonation: " + WindowsIdentity.GetCurrent().Name);

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
                              Loggers.ActiveDirectryLogger.Error("Impersonation running as application identity  {@Error}", new ApplicationException("Impersonation running as application identity"));

                          }
                          Loggers.ActiveDirectryLogger.Information("During impersonation: " + WindowsIdentity.GetCurrent().Name);
                          result = task.Invoke();
                      }
                      );

                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectryLogger.Error("Error running impersonated action " + impersonationUser.Username + " {@Error}", ex);
                }
                finally
                {
                    impersonatedToken?.Close();
                }
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectryLogger.Error("Error trying to impersonate " + impersonationUser.Username + " {@Error}", ex);
            }

            return result;
        }
        public async Task<string> RunProcess(string processPath, string arguments)
        {


            var output = "";
            try
            {

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = "C:\\",
                        FileName = processPath,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        LoadUserProfile = true,
                        UserName = impersonationUser?.Username,
                        Domain = impersonationUser?.FQDN,
                        Password = impersonationUser?.Password,
                    }

                };




                process.Start();

                // Reading the standard output stream of the process
                output = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();


            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectryLogger.Error("Error trying to impersonate " + impersonationUser.Username + " {@Error}", ex);
            }
            return output;

        }

    }
}