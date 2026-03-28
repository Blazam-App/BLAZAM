using BLAZAM.Helpers;
using System.Runtime.InteropServices;

namespace BLAZAM.Common.Data
{
    /// <summary>
    /// Provides functionality to execute commands as a different Windows user
    /// </summary>
    public class RunAs
    {
        private readonly WindowsImpersonationUser _user;

        #region P/Invoke

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessWithLogonW(
            string lpUsername,
            string lpDomain,
            IntPtr lpPassword,
            int dwLogonFlags,
            string? lpApplicationName,
            string? lpCommandLine,
            int dwCreationFlags,
            IntPtr lpEnvironment,
            string? lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            IntPtr lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out IntPtr phToken);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string? lpApplicationName,
            string? lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            int dwCreationFlags,
            IntPtr lpEnvironment,
            string? lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

 
    

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll")]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);

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

        private const int LOGON_WITH_PROFILE = 0x00000001;
        private const int CREATE_NO_WINDOW = 0x08000000;
        private const uint INFINITE = 0xFFFFFFFF;

        #endregion

        /// <summary>
        /// Creates a new RunAs instance with the specified user credentials
        /// </summary>
        /// <param name="user">The user credentials to use for executing commands</param>
        public RunAs(WindowsImpersonationUser user)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        /// <summary>
        /// Executes a command as the specified user via CreateProcessWithLogonW.
        /// Requires an interactive desktop/window station — not suitable for headless services.
        /// </summary>
        /// <param name="command">The command to execute (e.g., "cmd.exe /c exit")</param>
        /// <param name="timeoutMs">Timeout in milliseconds (default: 5000, use 0 for infinite)</param>
        /// <param name="workingDirectory">The working directory for the process (optional)</param>
        /// <returns>True if the command executed successfully with exit code 0</returns>
        /// <exception cref="InvalidOperationException">Thrown when the process returns a non-zero exit code.</exception>
        /// <exception cref="System.ComponentModel.Win32Exception">Thrown when the process fails to start.</exception>
        public bool ExecuteCommand(string command, uint timeoutMs = 5000, string? workingDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(command))
                throw new ArgumentException("Command cannot be null or empty", nameof(command));

            nint phPassword = 0;
            try
            {

                var si = new STARTUPINFO { cb = Marshal.SizeOf<STARTUPINFO>() };
                var domain = _user.FQDN ?? "";
                var username = _user.Username;

                phPassword = Marshal.SecureStringToGlobalAllocUnicode(_user.Password);

                // Attempt interactive logon first
                bool success = CreateProcessWithLogonW(
                    username,
                    domain,
                    phPassword,
                    LOGON_WITH_PROFILE,
                    null,
                    command,
                    CREATE_NO_WINDOW,
                    IntPtr.Zero,
                    workingDirectory,
                    ref si,
                    out PROCESS_INFORMATION pi);

                if (!success)
                {
                    int error = Marshal.GetLastWin32Error();
                    Loggers.ActiveDirectoryLogger.Warning(
                        "CreateProcessWithLogonW (batch) also failed for {@User}: {Command}, Error: {Error}",
                        username, command, error);

                    throw new System.ComponentModel.Win32Exception(error,
                        $"Failed to start process. Win32 Error Code: {error}");

                }

                Loggers.ActiveDirectoryLogger.Information(
                    "Command executed as {@User}: {Command}",
                    username,
                    command);

                // Wait for process to complete
                uint waitTime = timeoutMs == 0 ? INFINITE : timeoutMs;
                WaitForSingleObject(pi.hProcess, waitTime);

                // Retrieve process exit code
                GetExitCodeProcess(pi.hProcess, out uint exitCode);

                CloseHandle(pi.hProcess);
                CloseHandle(pi.hThread);

                if (exitCode != 0)
                {
                    var errorMsg = $"Command execution returned a non-zero exit code: {exitCode}";
                    Loggers.ActiveDirectoryLogger.Warning(errorMsg);
                    throw new InvalidOperationException(errorMsg);
                }

                return true;
            }
            finally
            {
                if (phPassword != 0)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(phPassword);
                }
            }
        }
    }
}