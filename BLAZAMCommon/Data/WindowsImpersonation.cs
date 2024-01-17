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
        static SafeAccessTokenHandle safeAccessTokenHandle;

        static WindowsImpersonationUser impersonationUser;


        public static SafeAccessTokenHandle ImpersonatedToken
        {
            get
            {
                if (safeAccessTokenHandle == null)
                {
                    // Call LogonUser to obtain a handle to an access token. 


                    bool returnValue = LogonUser(impersonationUser.Username, impersonationUser.FQDN!=null?impersonationUser.FQDN:"", impersonationUser.Password.ToPlainText(),
                            LOGON32_LOGON_NETWORK, LOGON32_PROVIDER_DEFAULT,
                            out safeAccessTokenHandle);
   
    
        

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
                }
                return safeAccessTokenHandle;

            }
            set => safeAccessTokenHandle = value;
        }

        const int LOGON32_PROVIDER_DEFAULT = 0;
        //This parameter causes LogonUser to create a primary token. 
        const int LOGON32_LOGON_INTERACTIVE = 2;
        const int LOGON32_LOGON_NETWORK = 9;


        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword,
    int dwLogonType, int dwLogonProvider, out SafeAccessTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        public WindowsImpersonation(WindowsImpersonationUser user)
        {
            impersonationUser = user;
        }
        public async Task<T> RunAsync<T>(Func<T> task) => await Task.Run(() => Run<T>(task));
        public T Run<T>(Func<T> task)
        {


            T result = default;

            try
            {
                if (ImpersonatedToken==null || ImpersonatedToken.IsInvalid) throw new ApplicationException("The impersonation user is invalid. Check settings.");

                //Console.WriteLine("Did LogonUser Succeed? " + (returnValue ? "Yes" : "No"));
                // Check the identity.
                Loggers.ActiveDirectryLogger.Information("Before impersonation: " + WindowsIdentity.GetCurrent().Name);

         

                WindowsIdentity.RunImpersonated(
                  ImpersonatedToken,
                  () =>
                  {
                      // Check the identity.
                      Loggers.ActiveDirectryLogger.Information("During impersonation: " + WindowsIdentity.GetCurrent().Name);
                      result = task.Invoke();
                  }
                  );

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
            try { 

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