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

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessWithTokenW(
            IntPtr hToken,
            int dwLogonFlags,
            string? lpApplicationName,
            string? lpCommandLine,
            int dwCreationFlags,
            IntPtr lpEnvironment,
            string? lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("userenv.dll", SetLastError = true)]
        private static extern bool CreateEnvironmentBlock(
            out IntPtr lpEnvironment, IntPtr hToken, bool bInherit);

        [DllImport("userenv.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyEnvironmentBlock(IntPtr lpEnvironment);

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
        private const int LOGON32_LOGON_BATCH = 4;
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int CREATE_NO_WINDOW = 0x08000000;
        private const int CREATE_UNICODE_ENVIRONMENT = 0x00000400;
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
                const int LOGON32_LOGON_INTERACTIVE = 2;

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
                        "CreateProcessWithLogonW (interactive) failed for {@User}: {Command}, Error: {Error}",
                        username, command, error);

                    // Fallback to batch logon if interactive logon fails
                    success = CreateProcessWithLogonW(
                        username,
                        domain,
                        phPassword,
                        LOGON32_LOGON_BATCH,
                        null,
                        command,
                        CREATE_NO_WINDOW,
                        IntPtr.Zero,
                        workingDirectory,
                        ref si,
                        out pi);

                    if (!success)
                    {
                        error = Marshal.GetLastWin32Error();
                        Loggers.ActiveDirectoryLogger.Warning(
                            "CreateProcessWithLogonW (batch) also failed for {@User}: {Command}, Error: {Error}",
                            username, command, error);

                        throw new System.ComponentModel.Win32Exception(error,
                            $"Failed to start process. Win32 Error Code: {error}");
                    }
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

        /// <summary>
        /// Launches a command as the specified user via CreateProcessWithLogonW
        /// without waiting for the process to complete. The child process is created
        /// by the Secondary Logon service (seclogon), so it is fully independent of
        /// the calling process and survives the caller's exit.
        ///
        /// <para>
        /// The resulting token is UAC-filtered (INTERACTIVE logon). The process will
        /// have the user's personal SID active for ALLOW checks, but the Administrators
        /// group SID is deny-only. Ensure the target user has explicit file-system ACLs
        /// on any directories the process needs to write to.
        /// </para>
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="workingDirectory">The working directory for the process (optional)</param>
        /// <returns>The process ID of the launched process</returns>
        /// <exception cref="System.ComponentModel.Win32Exception">Thrown when the process fails to start.</exception>
        public int LaunchDetached(string command, string? workingDirectory = null)
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

                if (success)
                {
                    int pid = pi.dwProcessId;
                    Loggers.ActiveDirectoryLogger.Information(
                        "Detached process launched as {@User}: {Command} (PID {Pid})",
                        username, command, pid);

                    // Release handles immediately — we intentionally do not wait
                    CloseHandle(pi.hProcess);
                    CloseHandle(pi.hThread);
                    return pid;
                }
                else
                {
                    int error = Marshal.GetLastWin32Error();
                    Loggers.ActiveDirectoryLogger.Warning(
                        "Failed to launch detached process as {@User}: {Command}, Error: {Error}",
                        username, command, error);
                    throw new System.ComponentModel.Win32Exception(error,
                        $"Failed to start detached process. Win32 Error Code: {error}");
                }
            }
            finally
            {
                if (phPassword != 0)
                    Marshal.ZeroFreeGlobalAllocUnicode(phPassword);
            }
        }

        /// <summary>
        /// Executes a command as the specified user via CreateProcessAsUser.
        /// Works in headless/non-interactive service contexts (IIS, Windows Service)
        /// without requiring a desktop or window station.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <param name="timeoutMs">Timeout in milliseconds (default: 5000, use 0 for infinite)</param>
        /// <param name="workingDirectory">The working directory for the process (optional)</param>
        /// <returns>True if the command executed successfully with exit code 0</returns>
        /// <exception cref="InvalidOperationException">Thrown when the process returns a non-zero exit code.</exception>
        /// <exception cref="System.ComponentModel.Win32Exception">Thrown when LogonUser or CreateProcessAsUser fails.</exception>
        public bool ExecuteCommandHeadless(string command, uint timeoutMs = 5000, string? workingDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(command))
                throw new ArgumentException("Command cannot be null or empty", nameof(command));

            nint phPassword = 0;
            IntPtr hToken = IntPtr.Zero;
            IntPtr envBlock = IntPtr.Zero;

            try
            {
                var domain = _user.FQDN ?? "";
                var username = _user.Username;
                phPassword = Marshal.SecureStringToGlobalAllocUnicode(_user.Password);

                // LOGON32_LOGON_BATCH works without an interactive session
                bool logonOk = LogonUser(
                    username, domain, phPassword,
                    LOGON32_LOGON_BATCH,
                    LOGON32_PROVIDER_DEFAULT,
                    out hToken);

                if (!logonOk)
                {
                    int err = Marshal.GetLastWin32Error();
                    Loggers.ActiveDirectoryLogger.Warning(
                        "LogonUser (batch) failed for {@User}, Error: {Error}", username, err);
                    throw new System.ComponentModel.Win32Exception(err,
                        $"LogonUser failed. Win32 Error Code: {err}");
                }

                // Build the user's environment so PATH, TEMP, etc. resolve correctly
                if (!CreateEnvironmentBlock(out envBlock, hToken, false))
                {
                    Loggers.ActiveDirectoryLogger.Warning(
                        "CreateEnvironmentBlock failed for {@User}; using inherited environment", username);
                    envBlock = IntPtr.Zero;
                }

                var si = new STARTUPINFO
                {
                    cb = Marshal.SizeOf<STARTUPINFO>(),
                    lpDesktop = ""   // empty string → non-interactive desktop
                };

                int flags = CREATE_NO_WINDOW;
                if (envBlock != IntPtr.Zero)
                    flags |= CREATE_UNICODE_ENVIRONMENT;

                bool success = CreateProcessWithTokenW(
                    hToken,
                    LOGON_WITH_PROFILE,
                    null,
                    command,
                    flags,
                    envBlock,
                    workingDirectory,
                    ref si,
                    out PROCESS_INFORMATION pi);

                if (success)
                {
                    Loggers.ActiveDirectoryLogger.Information(
                        "Headless command executed as {@User}: {Command}", username, command);

                    uint waitTime = timeoutMs == 0 ? INFINITE : timeoutMs;
                    WaitForSingleObject(pi.hProcess, waitTime);

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
                else
                {
                    int error = Marshal.GetLastWin32Error();
                    Loggers.ActiveDirectoryLogger.Warning(
                        "Failed to execute headless command as {@User}: {Command}, Error: {Error}",
                        username, command, error);
                    throw new System.ComponentModel.Win32Exception(error,
                        $"Failed to start process. Win32 Error Code: {error}");
                }
            }
            finally
            {
                if (phPassword != 0)
                    Marshal.ZeroFreeGlobalAllocUnicode(phPassword);
                if (envBlock != IntPtr.Zero)
                    DestroyEnvironmentBlock(envBlock);
                if (hToken != IntPtr.Zero)
                    CloseHandle(hToken);
            }
        }
    }
}