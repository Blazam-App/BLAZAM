using Cassia;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Logger;
using BLAZAM.Common.Data;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class ADComputerSessions : IDisposable
    {

        ITerminalServicesManager manager = new TerminalServicesManager();
        ITerminalServer server;
        private bool Polling;
        public List<IRemoteSession> ConnectedSessions = new List<IRemoteSession>();
        IADComputer Computer;

        public AppEvent ConnectedSessionsChanged { get; set; }
        public ADComputerSessions(IADComputer host)
        {
            Computer = host;
            if (Computer.IsOnline == true)
                RefreshSessions();
            else
                Computer.OnOnlineChanged += (status) => { if (status == true) RefreshSessions(); };

        }


        private void SessionDownEvent(IRemoteSession value)
        {

            ConnectedSessions.Remove(value);
            ConnectedSessionsChanged?.Invoke();



        }

        private void RefreshSessions()
        {
            if (!Polling)
            {
                if (Computer.IsOnline == true)
                {
                    Loggers.ActiveDirectryLogger.Information("Getting sessions for " + Computer);
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
                                   Loggers.ActiveDirectryLogger.Error("Error while collecting sessions for " + Computer, ex);
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
                           Loggers.ActiveDirectryLogger.Error("Error while connecting to TerminalServices on " + Computer, ex);
                           return false;
                       }
                   });
                }
             

            }

        }

        public void Dispose()
        {

            ConnectedSessions.Clear();

        }


    }
}