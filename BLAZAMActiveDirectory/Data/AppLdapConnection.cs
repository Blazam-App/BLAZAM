using System.DirectoryServices.Protocols;
using BLAZAM.Common.Data;
using BLAZAM.Logger;

namespace BLAZAM.ActiveDirectory.Data
{
    public class AppLdapConnection : IDisposable
    {
        public LdapConnection LdapConnection { get; set; }

        private readonly bool _startedTLS;
        internal readonly Guid Guid = Guid.NewGuid();
        private Timer? _keepAliveTime = null;
        public bool IsDisposed;
        public DateTime? Expires;

        public AppLdapConnection(LdapConnection ldapConnection, bool startedTLS)
        {
            if (ldapConnection == null) throw new ArgumentNullException("ldapConnection");
            LdapConnection = ldapConnection;
            _startedTLS = startedTLS;
            Random random = new Random();

            _keepAliveTime = new Timer(KeepAlive, null, random.Next(25000, 35000), random.Next(29000, 31000));

            ApplicationStatistics.AddLdapConnection(Guid);
        }

        private void KeepAlive(object? state)
        {
            var whoAmIRequest = new SearchRequest("", "(objectClass=*)", SearchScope.Base);
            try
            {
                var response = SendRequest(whoAmIRequest);
                if (response == null || response.ResultCode != ResultCode.Success)
                {
                    DisposeNow();
                }
            }
            catch (Exception ex)
            {
                DisposeNow();
            }

        }

        public DirectoryResponse SendRequest(DirectoryRequest request)
        {
            return LdapConnection.SendRequest(request);
        }

        ~AppLdapConnection()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (_keepAliveTime != null)
                    {
                        _keepAliveTime.Dispose();
                    }
                    try
                    {
                        if (_startedTLS)
                        {
                            LdapConnectionFactory.StopTls(LdapConnection);
                        }
                    }
                    catch (Exception ex)
                    {
                        Loggers.ActiveDirectoryLogger.Information(ex, "Error when stopping TLS");
                    }
                    finally
                    {
                        LdapConnection?.Dispose();
                    }
                }

                ApplicationStatistics.RemoveLdapConnection(Guid);

                IsDisposed = true;
            }
        }


        public void DisposeNow()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        public void Dispose()
        {
            Expires = DateTime.Now.AddMinutes(1);

        }
    }
}
