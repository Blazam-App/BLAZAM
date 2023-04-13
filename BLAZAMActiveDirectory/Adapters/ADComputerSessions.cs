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
        IADComputer _host;

        public AppEvent ConnectedSessionsChanged { get; set; }
        public ADComputerSessions(IADComputer host)
        {
            _host = host;
            RefreshSessions();

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

                Loggers.ActiveDirectryLogger.Information("Getting sessions for " + _host);
                Polling = true;
                var impersonation = _host.Directory.Impersonation;
                var success = impersonation.Run(() =>
               {
                   try
                   {
                       server = manager.GetRemoteServer(_host.CanonicalName);
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
                                       IRemoteSession s = new RemoteSession(session,_host);
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
                               Loggers.ActiveDirectryLogger.Error("Error while collecting sessions for " + _host, ex);
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
                       Loggers.ActiveDirectryLogger.Error("Error while connecting to TerminalServices on " + _host, ex);
                       return false;
                   }
               });


            }

        }

        public void Dispose()
        {

            ConnectedSessions.Clear();
            ConnectedSessions = null;

        }


    }
}