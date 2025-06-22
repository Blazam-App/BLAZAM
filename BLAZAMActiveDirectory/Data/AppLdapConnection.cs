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

        private Timer? _keepAliveTime = null;
        public bool IsDisposed;
        public DateTime? Expires;

        public AppLdapConnection(LdapConnection ldapConnection)
        {
            if (ldapConnection == null) throw new ArgumentNullException("ldapConnection");
            LdapConnection = ldapConnection;
            _keepAliveTime = new Timer(KeepAlive, null, 30000, 30000);

        }

        private void KeepAlive(object? state)
        {
            var whoAmIRequest = new SearchRequest("", "(objectClass=*)", System.DirectoryServices.Protocols.SearchScope.Base, "user");
            var response = SendRequest(whoAmIRequest);
            
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
                    LdapConnection?.SessionOptions.StopTransportLayerSecurity();
                    LdapConnection?.Dispose();
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
