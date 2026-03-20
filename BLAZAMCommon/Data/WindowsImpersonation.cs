using BLAZAM.Helpers;
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
        public WindowsImpersonationUser? ImpersonationUser { get; private set; }
        private readonly WindowsIdentity ApplicationIdentity;

        private SafeAccessTokenHandle GetImpersonatedToken()
        {
            nint phPassword = 0;
            try
            {
                if (ImpersonationUser == null)
                {
                    throw new AppException("Attempted to impersonate without an impersonation user");
                }
                //Use interactive logon
                var domain = ImpersonationUser.FQDN ?? "";
                var username = ImpersonationUser.Username;
                phPassword = Marshal.SecureStringToGlobalAllocUnicode(ImpersonationUser.Password);
                bool returnValue = LogonUser(username,
                        domain,
                        phPassword,
                        LOGON32_LOGON_INTERACTIVE,
                        LOGON32_PROVIDER_DEFAULT,
                        out safeAccessTokenHandle);



                if (!returnValue)
                {
                    int ret = Marshal.GetLastWin32Error();
                    Loggers.ActiveDirectoryLogger.Warning("LogonUser failed with error code : {0}", ret);
                    var exception = new System.ComponentModel.Win32Exception(ret);


                    throw new AuthenticationException(exception.Message);

                }
                return safeAccessTokenHandle;
            }
            finally
            {
                if (phPassword != 0)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(phPassword);
                }
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
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessAsUser(
            SafeAccessTokenHandle hToken,
            string? lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string? lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll")]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct STARTUPINFO
        {
            public int cb;
            public string? lpReserved;
            public string? lpDesktop;
            public string? lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessWithLogonW(
            string lpUsername,
            string lpDomain,
            IntPtr lpPassword,
            int dwLogonFlags,
            string lpApplicationName,
            string lpCommandLine,
            int dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        private const int LOGON_WITH_PROFILE = 0x00000001;
        private const int CREATE_NO_WINDOW = 0x08000000;

        /// <summary>
        /// Ensures the user profile exists by launching a minimal process as the user
        /// </summary>
        public void EnsureProfileExists()
        {
            if (ImpersonationUser == null) return;


           
            nint phPassword = 0;
            try
            {
                var si = new STARTUPINFO { cb = Marshal.SizeOf<STARTUPINFO>() };
                var domain = ImpersonationUser.FQDN ?? "";
                var username = ImpersonationUser.Username;

                phPassword = Marshal.SecureStringToGlobalAllocUnicode(ImpersonationUser.Password);

                // Launch cmd.exe /c exit - minimal process that creates profile
                bool success = CreateProcessWithLogonW(
                    username,
                    domain,
                    phPassword,
                    LOGON_WITH_PROFILE,  // This flag loads the profile
                    null,
                    "cmd.exe /c exit",
                    CREATE_NO_WINDOW,
                    IntPtr.Zero,
                    null,
                    ref si,
                    out PROCESS_INFORMATION pi);

                if (success)
                {
                    Loggers.ActiveDirectoryLogger.Information("Profile creation process launched for {@User}", username);
                    // Wait for process to complete (5 second timeout)
                    WaitForSingleObject(pi.hProcess, 5000);
                    CloseHandle(pi.hProcess);
                    CloseHandle(pi.hThread);
                }
                else
                {
                    int error = Marshal.GetLastWin32Error();
                    Loggers.ActiveDirectoryLogger.Warning("Failed to create profile process for {@User}: Error {Error}",
                        username, error);
                }
            }
            finally
            {
                if (phPassword != 0)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(phPassword);
                }
            }

        }

        /// <summary>
        /// Creates a new impersonation context under the provided <see cref="WindowsImpersonationUser"/>
        /// </summary>
        /// <param name="user"></param>
        public WindowsImpersonation(WindowsImpersonationUser? user)
        {
            ImpersonationUser = user;
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
            if (ImpersonationUser == null)
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
                          if (ImpersonationUser.Username != ApplicationIdentity.Name && impersonatedIdentity.Name.Equals(ApplicationIdentity.Name))
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
                    Loggers.ActiveDirectoryLogger.Information(ex, "The identity could not be mapped to a Windows account {@Impersonatee}", ImpersonationUser.Username);
                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectoryLogger.Error(ex, "Error running impersonated action {@Impersonatee}", ImpersonationUser.Username);
                }
                finally
                {
                    impersonatedToken.Close();
                }
            }
            catch (AuthenticationException ex)
            {
                Loggers.ActiveDirectoryLogger.Information(ex, "Bad credentials trying to impersonate user {@Username}", ImpersonationUser.Username);
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Information(ex, "Error trying to impersonate {@Impersonatee}", ImpersonationUser.Username);
            }

            return result;
        }


    }
}