using BLAZAM.Common.Data;
using BLAZAM.Logger;
using System.DirectoryServices.Protocols;

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
            if (ldapConnection == null)
            {
                throw new ArgumentNullException("ldapConnection");
            }

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

        private static ExpiringMemoryCache _cachedUpnSuffixes = new();
        internal List<string> GetForestAlternativeUpnSuffixes()
        {
            var _cachedValue = _cachedUpnSuffixes.GetValue<List<string>>();
            if (_cachedValue != null)
            {
                return _cachedValue;
            }
            var suffixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

         

            // 1. Query RootDSE for configurationNamingContext
            var rootDseRequest = new SearchRequest(
                distinguishedName: "",
                ldapFilter: "(objectClass=*)",
                searchScope: SearchScope.Base,
                attributeList: new[] { "configurationNamingContext" }
            );

            var rootDseResponse = (SearchResponse)LdapConnection.SendRequest(rootDseRequest);
            var configContext = rootDseResponse.Entries[0]
                .Attributes["configurationNamingContext"]?[0]?.ToString();

            if (string.IsNullOrWhiteSpace(configContext))
            {
                return suffixes.ToList();
            }

            // 2. Query CN=Partitions for uPNSuffixes
            var partitionsDn = $"CN=Partitions,{configContext}";
            var partitionsRequest = new SearchRequest(
                distinguishedName: partitionsDn,
                ldapFilter: "(objectClass=crossRefContainer)",
                searchScope: SearchScope.Base,
                attributeList: new[] { "uPNSuffixes" }
            );

            var partitionsResponse = (SearchResponse)SendRequest(partitionsRequest);
            if (partitionsResponse.Entries.Count > 0 &&
                partitionsResponse.Entries[0].Attributes.Contains("uPNSuffixes"))
            {
                foreach (var value in partitionsResponse.Entries[0].Attributes["uPNSuffixes"].GetValues(typeof(string)))
                {
                    if (value is string suffix && !string.IsNullOrWhiteSpace(suffix))
                    {
                        suffixes.Add(suffix);
                    }
                }
            }
            var suffixList = suffixes.ToList();
            _cachedUpnSuffixes.SetValue(suffixList,TimeSpan.FromMinutes(5));
            return suffixList;
        }
        ~AppLdapConnection()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            InternalDispose(disposing: false);
        }
        protected virtual void InternalDispose(bool disposing)
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
            InternalDispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        public void Dispose()
        {
            Expires = DateTime.Now.AddMinutes(1);

        }
    }
}
