using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using Cassia;
using Serilog;
using System;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class RemoteSession : IRemoteSession
    {
        ITerminalServicesSession _session;
        ITerminalServicesSession Session
        {
            get => _session; set
            {
                if (_session == value) return;
                _session = value;
                WindowsImpersonation.Run(() =>
                {
                    if (!_session.Server.IsOpen)
                        _session.Server.Open();
                    try
                    {

                        _user = _session.UserAccount;
                    }
                    catch
                    {

                    }
                    try
                    {

                        _sessionId = _session.SessionId;

                    }
                    catch
                    {

                    }
                    try
                    {

                        _idleTime = _session.IdleTime;

                    }
                    catch
                    {

                    }
                    try
                    {

                        _connectionState = _session.ConnectionState;

                    }
                    catch
                    {

                    }
                    try
                    {

                        _connectTime = _session.ConnectTime;
                    }
                    catch
                    {

                    }
                    try
                    {

                        _clientIPAddress = _session.ClientIPAddress;

                    }
                    catch
                    {

                    }



                    return true;

                });
            }
        }
        public AppEvent<IRemoteSession> OnSessionDown { get; set; }
        public AppEvent<IRemoteSession> OnSessionUpdated { get; set; }

        Timer t;
        public RemoteSession(ITerminalServicesSession session)
        {
            Session = session;
            t = new Timer(Tick, null, 10000, 10000);
            // Monitor();


        }

        private void Tick(object? state)
        {
            GetNewSessionState();
        }

        public ITerminalServer Server => _session.Server;

        NTAccount _user;
        public NTAccount User
        {
            get
            {
                return _user;

            }
        }
        DateTime? _connectTime;
        public DateTime? ConnectTime
        {
            get
            {
                return _connectTime;

            }
        }
        DateTime? _loginTime;
        public DateTime? LoginTime
        {
            get
            {
                return _loginTime;

            }
        }
        TimeSpan? _idleTime;
        public TimeSpan? IdleTime
        {
            get
            {
                return _idleTime;

            }
        }
        Cassia.ConnectionState _connectionState;
        public Cassia.ConnectionState ConnectionState
        {
            get
            {
                return _connectionState;

            }
        }
        IPAddress _clientIPAddress;
        public IPAddress ClientIPAddress
        {
            get
            {
                return _clientIPAddress;

            }
        }
        int _sessionId;
        public int SessionId
        {
            get
            {
                return _sessionId;

            }
        }

        public bool Monitoring { get; private set; }

        public void Logoff(bool synchronous = false)
        {
            WindowsImpersonation.Run(() =>
            {
                if (!_session.Server.IsOpen)
                    _session.Server.Open();
                _session.Logoff(synchronous);
                _session.Server.Close();
                return true;

            });


        }
        public void Disconnect(bool synchronous = false)
        {
            WindowsImpersonation.Run(() =>
            {
                if (!_session.Server.IsOpen)
                    _session.Server.Open();
                _session.Disconnect(synchronous);
                _session.Server.Close();
                return true;
            });



        }





        private void GetNewSessionState()
        {
            try
            {
                WindowsImpersonation.Run(() =>
                {
                    if (!_session.Server.IsOpen)
                        _session.Server.Open();
                    if (_session.Server.IsOpen)
                    {
                        int id = _session.SessionId;
                        ITerminalServicesSession updated = _session.Server.GetSession(id);
                        if (updated != null)

                            Task.Run(() => { Session = updated; OnSessionUpdated?.Invoke(this); });

                        //session.Server.Close();
                    }
                    return true;
                });
            }
            catch (Win32Exception ex)
            {
                if (ex.Message == "The system cannot find the file specified.")
                {

                    OnSessionDown?.Invoke(this);
                }


            }
            catch (Exception ex)
            {
                Log.Error("An error occured while refreshing a computer session state.", ex);
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is IRemoteSession other)
            {
                if (other.SessionId.Equals(SessionId) && other.Server.ServerName.Equals(Server.ServerName))
                {
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (SessionId + Server.ServerName).GetHashCode();
        }
    }
}