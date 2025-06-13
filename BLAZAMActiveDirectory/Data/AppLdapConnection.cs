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
        public bool IsDisposed;
        public DateTime? Expires;

        public AppLdapConnection(LdapConnection ldapConnection)
        {
            LdapConnection = ldapConnection;
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
                    // TODO: dispose managed state (managed objects)
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
