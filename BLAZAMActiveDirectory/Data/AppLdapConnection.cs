using BLAZAM.Logger;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Data
{
    public class AppLdapConnection:IDisposable
    {
        public LdapConnection LdapConnection { get; set; }

        private readonly bool _startedTLS;
        private Timer? _keepAliveTime = null;
        public bool IsDisposed;
        public DateTime? Expires;

        public AppLdapConnection(LdapConnection ldapConnection, bool startedTLS)
        {
            if (ldapConnection == null) throw new ArgumentNullException("ldapConnection");
            LdapConnection = ldapConnection;
            _startedTLS = startedTLS;
            Random random = new Random();
            
            _keepAliveTime = new Timer(KeepAlive, null, random.Next(25000,35000), random.Next(29000,31000));

        }

        private void KeepAlive(object? state)
        {
            var whoAmIRequest = new SearchRequest("", "(objectClass=*)", System.DirectoryServices.Protocols.SearchScope.Base);
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

        public DirectoryResponse SendRequest(DirectoryRequest request) {
            return LdapConnection.SendRequest(request);    
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
                    // TODO: dispose managed state (managed objects)
                    try
                    {
                        if (_startedTLS)
                        {
                            //SecureLdapConnector.StopTls(LdapConnection);
                        }
                    }
                    catch(Exception ex) {
                        Loggers.ActiveDirectoryLogger.Information(ex, "Error when stopping TLS");
                    }
                    finally
                    {
                        LdapConnection?.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                IsDisposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~AppLdapConnection()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
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
