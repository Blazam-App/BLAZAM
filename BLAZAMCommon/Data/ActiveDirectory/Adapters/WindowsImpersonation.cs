using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Exceptions;
using BLAZAM.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32.SafeHandles;
using Serilog;
namespace BLAZAM.Common.Data.ActiveDirectory.Models
{
    public class WindowsImpersonation
    {
        static SafeAccessTokenHandle safeAccessTokenHandle;

        public static SafeAccessTokenHandle ImpersonatedToken
        {
            get
            {
                if (safeAccessTokenHandle == null)
                {
                    // Call LogonUser to obtain a handle to an access token. 
                    bool returnValue = LogonUser(DatabaseCache.ActiveDirectorySettings.Username, DatabaseCache.ActiveDirectorySettings.FQDN, DatabaseCache.ActiveDirectorySettings.Password,
                        LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
                        out safeAccessTokenHandle);

                    if (false == returnValue)
                    {
                        int ret = Marshal.GetLastWin32Error();
                        Loggers.ActiveDirectryLogger.Error("LogonUser failed with error code : {0}", ret);
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

        public AppDatabaseFactory Factory { get; }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
    int dwLogonType, int dwLogonProvider, out SafeAccessTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        public WindowsImpersonation()
        {
            
        }

        public static T Run<T>(Func<T> task)
        {






            //Console.WriteLine("Did LogonUser Succeed? " + (returnValue ? "Yes" : "No"));
            // Check the identity.
            Loggers.ActiveDirectryLogger.Debug("Before impersonation: " + WindowsIdentity.GetCurrent().Name);

            // Note: if you want to run as unimpersonated, pass
            //       'SafeAccessTokenHandle.InvalidHandle' instead of variable 'safeAccessTokenHandle'
            T success = default(T);
            try
            {
                WindowsIdentity.RunImpersonated(
                  ImpersonatedToken,
                  () =>
                  {
                      // Check the identity.
                      Loggers.ActiveDirectryLogger.Debug("During impersonation: " + WindowsIdentity.GetCurrent().Name);
                      success = task.Invoke();
                  }
                  );

            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectryLogger.Error("Error trying to impersonate "+DatabaseCache.ActiveDirectorySettings.Username, ex);
            }
            return success;
        }
        public static async Task<string> RunProcess(string processPath, string arguments)
        {

            //process.StartInfo.UserName = DatabaseCache.ActiveDirectorySettings.Username+"@"+ DatabaseCache.ActiveDirectorySettings.FQDN;
            //process.StartInfo.Password = DatabaseCache.ActiveDirectorySettings.Password.ToSecureString();
            var output = "";


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
                    UserName = DatabaseCache.ActiveDirectorySettings?.Username,
                    Domain = DatabaseCache.ActiveDirectorySettings?.FQDN,
                    Password = DatabaseCache.ActiveDirectorySettings?.Password.ToSecureString(),
                }

            };


          

            process.Start();

            // Reading the standard output stream of the process
            output = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();


            
            return output;

            throw new Exception("Unknown exception impersonating process");
        }

    }
}