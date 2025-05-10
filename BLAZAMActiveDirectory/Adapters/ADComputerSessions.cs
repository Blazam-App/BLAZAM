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
        public List<IRemoteSession> ConnectedSessions { get; set; } = new();
        private readonly IADComputer Computer;

        public AppDelegate ConnectedSessionsChanged { get; set; }
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

            if (!Polling && Computer.IsOnline == true)
            {
                Loggers.ActiveDirectoryLogger.Information("Getting sessions for " + Computer);
                Polling = true;
                var impersonation = Computer.Directory.Impersonation;
                var success = impersonation.Run(() =>
               {
                   try
                   {
                       server = manager.GetRemoteServer(Computer.CanonicalName);
                       try
                       {
                           server.Open();

                           try
                           {
                               foreach (ITerminalServicesSession session in server.GetSessions())
                               {
                                   if (session.UserAccount != null)
                                   {
                                       if (!session.Server.IsOpen)
                                           session.Server.Open();
                                       IRemoteSession s = new RemoteSession(session, Computer);
                                       s.OnSessionDown += SessionDownEvent;
                                       if (!ConnectedSessions.Contains(s))
                                       {
                                           ConnectedSessions.Add(s);
                                           ConnectedSessionsChanged?.Invoke();
                                       }
                                   }
                               }

                           }
                           catch (Win32Exception ex)
                           {
                               Loggers.ActiveDirectoryLogger.Error("Error while collecting sessions for " + Computer + " {@Error}", ex);
                           }
                       }
                       catch
                       {

                       }


                       Polling = false;
                       return true;
                   }
                   catch (Exception ex)
                   {
                       Loggers.ActiveDirectoryLogger.Error("Error while connecting to TerminalServices on " + Computer + " {@Error}", ex);
                       return false;
                   }
               });
            }

        }

        public void Dispose()
        {
            foreach (var session in ConnectedSessions)
            {
                session.Dispose();
            }
            ConnectedSessions.Clear();
            server.Close();
        }


    }
}