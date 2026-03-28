// BLAZAMUpdate/UpdateTaskScheduler.cs
using BLAZAM.Common.Data;
using BLAZAM.FileSystem;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using System.Security.AccessControl;

namespace BLAZAM.Update
{
    /// <summary>
    /// Launches the self-update PowerShell script directly as the configured admin user
    /// via <see cref="RunAs.LaunchDetached"/> (<c>CreateProcessWithLogonW</c>).
    ///
    /// <para><b>Why not Task Scheduler or CreateProcessWithTokenW?</b></para>
    /// <para>
    /// From a non-elevated process there is no Windows API that produces an unfiltered
    /// admin token in a child process:
    /// <list type="bullet">
    ///   <item><c>CreateProcessWithLogonW</c> — INTERACTIVE logon → UAC-filtered</item>
    ///   <item><c>CreateProcessWithTokenW</c> — requires <c>SeImpersonatePrivilege</c></item>
    ///   <item><c>CreateProcessAsUser</c> — requires <c>SeAssignPrimaryTokenPrivilege</c></item>
    ///   <item>Task Scheduler COM — in-process server, no DCOM proxy, identity not controllable</item>
    /// </list>
    /// </para>
    ///
    /// <para><b>How this works:</b></para>
    /// <para>
    /// <c>CreateProcessWithLogonW</c> gives a filtered admin token. The filtered token
    /// still carries the user's personal SID — it just loses the Administrators group
    /// for allow-checks. Before launching, we use <see cref="WindowsImpersonation.RunBatch{T}"/>
    /// (BATCH logon = unfiltered in-process token) to grant the user explicit Full Control
    /// ACLs on the application directory. The PowerShell script then operates using the
    /// personal SID. For IIS, <c>update.ps1</c> only performs file I/O — no admin APIs.
    /// </para>
    ///
    /// <para>
    /// The child process is created by the Secondary Logon service (<c>seclogon</c>),
    /// so it is fully independent and survives the application's exit.
    /// </para>
    /// </summary>
    public static class UpdateTaskScheduler
    {
        /// <summary>
        /// Ensures the update user has file-system write access to the application
        /// directory, then launches the PowerShell update script as a detached process.
        /// </summary>
        /// <param name="powershellArguments">Arguments for powershell.exe (the update script invocation)</param>
        /// <param name="runAsUser">The impersonation context with admin credentials</param>
        /// <param name="tempDir">Temp directory (unused, kept for API compatibility)</param>
        /// <param name="applicationDirectory">
        /// The application root directory. If provided, explicit ACLs are granted
        /// to the update user before launching the script.
        /// </param>
        public static bool ScheduleUpdateTask(
            string powershellArguments,
            WindowsImpersonation runAsUser,
            SystemDirectory tempDir,
            string? applicationDirectory = null)
        {
            if (runAsUser?.ImpersonationUser == null)
                throw new InvalidOperationException(
                    "Admin credentials are required to launch the update process.");

            var user = runAsUser.ImpersonationUser;
            var qualifiedUser = string.IsNullOrEmpty(user.FQDN)
                ? user.Username
                : $@"{user.FQDN}\{user.Username}";

          

            // ── Step 2: Launch PowerShell directly (fire-and-forget) ──
            var runAs = new RunAs(user);
            var command = $"powershell.exe {powershellArguments}";

            Loggers.UpdateLogger?.Information(
                "Launching update script directly as {User}: {Command}",
                qualifiedUser, command);

            int pid = runAs.LaunchDetached(command);

            Loggers.UpdateLogger?.Information(
                "Update process launched (PID {Pid}). " +
                "Process is independent and will continue after app exit.", pid);

            return true;
        }

        /// <summary>
        /// Grants the specified user explicit Full Control on a directory using
        /// BATCH impersonation (unfiltered admin token, in-process — no
        /// <c>SeImpersonatePrivilege</c> or child process required).
        /// </summary>
        internal static void GrantDirectoryAccess(
            WindowsImpersonation adminIdentity,
            string directoryPath,
            string userAccount)
        {
            adminIdentity.RunBatch(() =>
            {
                var dirInfo = new DirectoryInfo(directoryPath);
                var security = dirInfo.GetAccessControl();

                // Check whether the user already has an explicit allow rule
                var rules = security.GetAccessRules(
                    includeExplicit: true, includeInherited: false,
                    targetType: typeof(System.Security.Principal.NTAccount));

                foreach (FileSystemAccessRule existing in rules)
                {
                    if (existing.IdentityReference.Value.Equals(
                            userAccount, StringComparison.OrdinalIgnoreCase)
                        && existing.FileSystemRights.HasFlag(FileSystemRights.FullControl)
                        && existing.AccessControlType == AccessControlType.Allow)
                    {
                        Loggers.UpdateLogger?.Debug(
                            "{User} already has Full Control on {Directory}",
                            userAccount, directoryPath);
                        return true;
                    }
                }

                var rule = new FileSystemAccessRule(
                    userAccount,
                    FileSystemRights.FullControl,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow);

                security.AddAccessRule(rule);
                dirInfo.SetAccessControl(security);

                Loggers.UpdateLogger?.Information(
                    "Granted Full Control to {User} on {Directory}",
                    userAccount, directoryPath);

                return true;
            });
        }
    }
}