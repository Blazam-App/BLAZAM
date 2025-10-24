using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Logger;
using Cassia;
using System.ComponentModel;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class ADComputerSessions : IDisposable
    {
        private readonly ITerminalServicesManager manager = new TerminalServicesManager();
        private ITerminalServer server;
        private bool Polling;
        public List<IRemoteSession> ConnectedSessions { get; set; } = [];
        private readonly IADComputer Computer;

        public AppEvent ConnectedSessionsChanged { get; set; } = new();
        public ADComputerSessions(IADComputer host)
        {
            Computer = host;
            if (Computer.IsOnline == true)
                RefreshSessions();
            else
                Computer.OnOnlineChanged += (status) => { if (status) RefreshSessions(); };

        }


        private void SessionDownEvent(IRemoteSession value)
        {

            ConnectedSessions.Remove(value);
            ConnectedSessionsChanged?.Invoke();



        }

        private void RefreshSessions()
        {
            // 1. Use guard clause to check if polling is already in progress or if the computer is offline.
            if (Polling || Computer.IsOnline != true)
            {
                return;
            }

            Loggers.ActiveDirectoryLogger.Information("Getting sessions for {Computer}", Computer);
            Polling = true;

            try
            {
                var impersonation = Computer.Directory.Impersonation;
                // 2. Delegate the core logic to a separate method.
                impersonation.Run(() => GetAndProcessSessions());
            }
            finally
            {
                // 3. Use a 'finally' block to guarantee state is reset.
                Polling = false;
            }
        }

        /// <summary>
        /// Connects to the remote server and processes all terminal sessions.
        /// This contains the error handling for the remote connection.
        /// </summary>
        private bool GetAndProcessSessions()
        {
            try
            {
                var server = manager.GetRemoteServer(Computer.CanonicalName);
                server.Open();

                foreach (ITerminalServicesSession session in server.GetSessions())
                {
                    // 4. Delegate the logic for a single session to another method.
                    ProcessSingleSession(session);
                }
                return true; // Return true if sessions were successfully collected.
            }
            catch (Win32Exception ex) // Catches specific session collection errors
            {
                Loggers.ActiveDirectoryLogger.Information(ex, "Error while collecting sessions for {Computer}", Computer);
            }
            catch (Exception ex) // Catches general connection errors
            {
                Loggers.ActiveDirectoryLogger.Error(ex, "Error connecting to TerminalServices on {Computer}", Computer.CanonicalName);
            }
            return false; // Return false if there was an error.
        }

        /// <summary>
        /// Processes an individual terminal session.
        /// </summary>
        private void ProcessSingleSession(ITerminalServicesSession session)
        {
            // Use guard clauses to skip irrelevant or existing sessions.
            if (session.UserAccount == null)
            {
                return;
            }

            var remoteSession = new RemoteSession(session, Computer);
            if (ConnectedSessions.Contains(remoteSession))
            {
                return;
            }

            // Logic for adding a new session.
            if (!session.Server.IsOpen)
            {
                session.Server.Open();
            }

            remoteSession.OnSessionDown += SessionDownEvent;
            ConnectedSessions.Add(remoteSession);
            ConnectedSessionsChanged?.Invoke();
        }

        public void Dispose()
        {
            foreach (var session in ConnectedSessions)
            {
                session.Dispose();
            }
            ConnectedSessions.Clear();
            server?.Close();
        }


    }
}