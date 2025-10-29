using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Data;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Exceptions;
using BLAZAM.Common.Helpers;
using BLAZAM.Database.Models;
using BLAZAM.Global.Enums;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using BLAZAM.Notifications.Services;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;

namespace BLAZAM.ActiveDirectory
{
    public class ActiveDirectoryContext : IActiveDirectoryContext
    {
        private static LdapConnectionFactory LdapConnectionFactory { get; set; }
        public DomainControllerEventLogReader EventLogReader { get; private set; }
        public ActiveDirectoryUserState? CurrentUser
        {
            get
            {
                if (_currentUser != null)
                {
                    return _currentUser;
                }

                return null;
            }
            set => _currentUser = value;
        }
        private CancellationTokenSource? _connectionCTS = new();

        private const string LDAP_PROTO = "LDAP://";
        private readonly WmiFactory _wmiFactory;
        private readonly IEncryptionService _encryption;
        private readonly INotificationPublisher _notificationPublisher;
        private static ActiveDirectoryContext _systemInstance;
        public static ActiveDirectoryContext SystemInstance { get => _systemInstance; }

        public int FailedConnectionAttempts { get; set; } = 0;

        private AuthType AuthType
        {
            get
            {
                AuthType _authType = AuthType.Negotiate;


                return _authType;

            }
        }


        /// <summary>

        /// </summary>
        public IDirectoryEntry? AppRootDirectoryEntry { get; private set; }

        /// <summary>
        /// The domain directory entry root
        /// </summary>
        /// <remarks>
        /// Caution should be used when providing this to the UI
        /// </remarks>
        public IDirectoryEntry RootDirectoryEntry { get; private set; }


        /// <summary>
        /// Initializes the applications Active Directory connection. It takes the information
        /// from the ActiveDirectorySetting table in the database and uses them to configure the
        /// connection.
        /// 
        /// </summary>
        /// <param name="context"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public ActiveDirectoryContext(IAppDatabaseFactory factory,
            IEncryptionService encryptionService,
            INotificationPublisher notificationPublisher
            )
        {
            if (LdapConnectionFactory == null)
            {
                LdapConnectionFactory = new();
            }
            _wmiFactory = new(this);
            _encryption = encryptionService;
            _notificationPublisher = notificationPublisher;
            Factory = factory;
            SetSystemInstance(this);
            EventLogReader = new(this);
            Task.Run(async () =>
            {
                using var connection = await CheckConnectionAsync();
                if (!_initializedConnections)
                {
                    _initializedConnections = true;

                    var connections = new List<AppLdapConnection?>();
                    for (int i = 0; i < LdapConnectionFactory.PoolSize - 1; i++)
                    {
                        var initconnection = LdapConnectionFactory.Connect(ConnectionSettings);
                        connections.Add(initconnection);
                    }
                    foreach (var conn in connections)
                    {
                        conn?.Dispose();
                    }
                }
                if (connection != null)
                {
                    TryGetDomainControllers();
                }
            });

            Users = new ADUserSearcher(this);
            Contacts = new ADContactSearcher(this);
            Groups = new ADGroupSearcher(this);
            OUs = new ADOUSearcher(this);
            Printers = new ADPrinterSearcher(this);
            BitLocker = new ADBitLockerSearcher(this);
            Computers = new ADComputerSearcher(this, _wmiFactory);
        }

        /// <summary>
        /// Used for factory creation of session scoped contexts.
        /// </summary>
        /// <param name="activeDirectoryContextSeed"></param>
        public ActiveDirectoryContext(ActiveDirectoryContext activeDirectoryContextSeed)
        {
            _encryption = activeDirectoryContextSeed._encryption;
            _notificationPublisher = activeDirectoryContextSeed._notificationPublisher;
            Factory = activeDirectoryContextSeed.Factory;
            ConnectionSettings = activeDirectoryContextSeed.ConnectionSettings;
            RootDirectoryEntry = activeDirectoryContextSeed.RootDirectoryEntry;
            AppRootDirectoryEntry = activeDirectoryContextSeed.AppRootDirectoryEntry;
            _wmiFactory = activeDirectoryContextSeed._wmiFactory;
            Status = activeDirectoryContextSeed.Status;
            EventLogReader = activeDirectoryContextSeed.EventLogReader;
            DomainControllers = activeDirectoryContextSeed.DomainControllers;

            Users = new ADUserSearcher(this);
            Contacts = new ADContactSearcher(this);
            Groups = new ADGroupSearcher(this);
            OUs = new ADOUSearcher(this);
            Printers = new ADPrinterSearcher(this);
            BitLocker = new ADBitLockerSearcher(this);
            Computers = new ADComputerSearcher(this, activeDirectoryContextSeed._wmiFactory);

        }

        public IDirectoryEntry GetDirectoryEntry(string? baseDN = null)
        {
            if (baseDN == null || baseDN == "")
            {
                baseDN = ConnectionSettings?.ApplicationBaseDN;
            }

            return new LdapDirectoryEntry(baseDN, this);
        }
        /// <summary>
        /// Gets the root entry for deleted objects in Active Directory
        /// </summary>
        /// <returns></returns>
        public IDirectoryEntry GetDeleteObjectsEntry()
        {
            if (ConnectionSettings == null || ConnectionSettings.FQDN == null)
                throw new InvalidOperationException("Connection settings or FQDN not available to construct Deleted Objects path.");
            string deletedObjectsDN = "CN=Deleted Objects," + ConnectionSettings.FQDN.FqdnToDN();
            return new LdapDirectoryEntry(deletedObjectsDN, this);
        }





        public IADUserSearcher Users { get; }

        public IADContactSearcher Contacts { get; }

        public IADGroupSearcher Groups { get; }

        public IADOUSearcher OUs { get; }

        public IADPrinterSearcher Printers { get; }

        public IADComputerSearcher Computers { get; }

        public IADBitLockerSearcher BitLocker { get; }

        private IDatabaseContext? _context { get; set; }


        public bool PortOpen
        {
            get
            {
                if (ConnectionSettings != null
                    && ConnectionSettings.ServerAddress != null
                    && ConnectionSettings.ServerAddress != ""
                    && ConnectionSettings.ServerPort != 0)
                {
                    return NetworkTools.IsPortOpen(ConnectionSettings.ServerAddress, ConnectionSettings.ServerPort);
                }

                return false;
            }
        }
        private DirectoryConnectionStatus _status = DirectoryConnectionStatus.Connecting;
        private ActiveDirectoryUserState? _currentUser;
        private bool _keepAlive;
        private static bool _initializedConnections;

        public DirectoryConnectionStatus Status
        {
            get => _status; set
            {
                if (_status == value)
                {
                    return;
                }

                _status = value;
                OnStatusChanged?.Invoke(_status);
            }
        }
        public AppDelegate<DirectoryConnectionStatus>? OnStatusChanged { get; set; }


        public Exception? ConnectionException { get; set; }

        public IAppDatabaseFactory Factory { get; private set; }

        public ADSettings ConnectionSettings { get; private set; }


        public WindowsImpersonation? Impersonation
        {
            get
            {
                return ConnectionSettings?.CreateDirectoryAdminImpersonator();
            }
        }

        private static void SetSystemInstance(ActiveDirectoryContext context)
        {
            _systemInstance = context;
        }
        public async Task<AppLdapConnection> GetConnectionAsync()
        {
            if (ConnectionSettings == null)
            {
                throw new InvalidOperationException("Active Directory Connection Settings are not configured.");
            }
            return await LdapConnectionFactory.ConnectAsync(ConnectionSettings);
        }
        public AppLdapConnection GetConnection()
        {
            if (ConnectionSettings == null)
            {
                throw new InvalidOperationException("Active Directory Connection Settings are not configured.");
            }
            return LdapConnectionFactory.Connect(ConnectionSettings);
        }


        public async Task<AppLdapConnection?> CheckConnectionAsync()
        {
            Status = DirectoryConnectionStatus.Connecting;
            return await Task.Run(() =>
            {
                return CheckConnect();

            });

        }
        public async Task CancelCheckConnection()
        {
            if (_connectionCTS != null)
            {
                await _connectionCTS.CancelAsync();
            }
            _connectionCTS?.Dispose();
            _connectionCTS = new();
        }
        /// <summary>
        /// Attempts a connection to the Active Directory server
        /// </summary>
        public AppLdapConnection? CheckConnect()
        {

            //Set status flag
            Status = DirectoryConnectionStatus.Connecting;

            Loggers.ActiveDirectoryLogger.Information("Initiating Active Directory connection");
            try
            {
                ADSettings? ad;
                using (var context = Factory.CreateDbContext())
                {

                if (IsCancelRequested)
                {
                    return null;
                }
                    ConnectDatabase(context);


                if (IsCancelRequested)
                {
                    return null;
                }
                    GetConnectionSettings(context, out ad);
                }

                if (IsCancelRequested)
                {
                    return null;
                }

                PerformNetworkTests(ad);


                if (IsCancelRequested)
                {
                    return null;
                }

                InitializeDirectoryEntries(ad);

                if (IsCancelRequested)
                {
                    return null;
                }
              

                return CreateConnection(ad);


            }
            catch (UnresolvableAddressException ex)
            {
                ConnectionException = ex;

                Loggers.ActiveDirectoryLogger.Warning(ex, "Unable to resolve Active Directory server address");
                Status = DirectoryConnectionStatus.ServerDown;
                if (FailedConnectionAttempts < 10)
                {
                    FailedConnectionAttempts++;
                }
            }
            catch (DirectoryOperationException ex)
            {
                ConnectionException = ex;

                Loggers.ActiveDirectoryLogger.Warning(ex, "Error connecting to Active Directory");

                Status = DirectoryConnectionStatus.BadConfiguration;
                if (FailedConnectionAttempts < 10)
                {
                    FailedConnectionAttempts++;
                }
            }
            catch (CryptographicException ex)
            {
                ConnectionException = ex;

                Loggers.ActiveDirectoryLogger.Warning(ex, "Unable to decrypt Active Directory password");
                Status = DirectoryConnectionStatus.EncryptionError;
                if (FailedConnectionAttempts < 10)
                {
                    FailedConnectionAttempts++;
                }
            }
            catch (COMException ex)
            {
                ConnectionException = ex;
                switch (ex.HResult)
                {

                    case -2147023436:
                        Loggers.ActiveDirectoryLogger.Information(ex, "Timeout connecting to Active Directory");
                        Status = DirectoryConnectionStatus.ServerDown;
                        break;
                    case -2147016646:
                        Loggers.ActiveDirectoryLogger.Information(ex, "Encrypted connection error to Active Directory");

                        Status = DirectoryConnectionStatus.EncryptionError;
                        break;
                    default:
                        Loggers.ActiveDirectoryLogger.Warning(ex, "Unexpected Error connecting to Active Directory");
                        Status = DirectoryConnectionStatus.ServerDown;
                        break;
                }
                if (FailedConnectionAttempts < 10)
                {
                    FailedConnectionAttempts++;
                }
            }
            catch (CriticalActiveDirectoryException ex)
            {
                ConnectionException = ex;
                Status = DirectoryConnectionStatus.BadConfiguration;
                if (FailedConnectionAttempts < 10)
                    FailedConnectionAttempts++;
            }
            catch (Exception ex)
            {
                ConnectionException = ex;

                switch (ex.HResult)
                {
                    case -2147016646:
                        Loggers.ActiveDirectoryLogger.Information(ex, "Encrypted connection error to Active Directory");

                        Status = DirectoryConnectionStatus.EncryptionError;
                        break;
                    case -2147023570:
                        Loggers.ActiveDirectoryLogger.Information(ex, "Bad credentials for Active Directory");

                        Status = DirectoryConnectionStatus.BadCredentials;
                        break;
                    default:
                        Loggers.ActiveDirectoryLogger.Information(ex, "Unexpected Error connecting to Active Directory");
                        Status = DirectoryConnectionStatus.ServerDown;
                        break;
                }
                if (FailedConnectionAttempts < 10)
                {
                    FailedConnectionAttempts++;
                }
            }
            if (IsCancelRequested == false && Status != DirectoryConnectionStatus.OK)
            {
                Task.Delay(5000).Wait();
                return CheckConnect();
            }

            return null;
        }

        private DirectoryContext DirectoryContext => new(
    DirectoryContextType.Domain,
    ConnectionSettings.FQDN,
    ConnectionSettings.Username,
    ConnectionSettings.Password.Decrypt()
    );

        /// <summary>
        /// Tries to get the domain controllers by connecting to the domain from the web server
        /// </summary>
        /// <remarks>
        /// If the web host cannot contact the domain directly via DNS this will not populate <see cref="DomainControllers"/>
        /// </remarks>
        private void TryGetDomainControllers()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }
            try
            {
                //Clear local list of domain controllers
                DomainControllers.Clear();

                foreach (DomainController dc in Domain.GetDomain(DirectoryContext).DomainControllers)
                {
                    DomainControllers.Add(dc.Name);
                }
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Warning("Could not get domain controllers directly {@Error}", ex);
            }

        }

        private bool IsCancelRequested
        {
            get
            {
                return _connectionCTS != null && _connectionCTS.IsCancellationRequested;
            }
        }
        private void GetConnectionSettings(IDatabaseContext context, out ADSettings? ad)
        {
            //Ok get the latest settings
            ad = context?.ActiveDirectorySettings.FirstOrDefault();
            if (IsCancelRequested)
            {
                return;
            }

            if (ad == null)
            {
                Status = DirectoryConnectionStatus.UnreachableConfiguration;
                if (FailedConnectionAttempts < 10)
                {
                    FailedConnectionAttempts++;
                }
                return;
            }
            ConnectionSettings = ad;

            Loggers.ActiveDirectoryLogger.Information("Active Directory settings found in database. {@DirectorySettings}", ad);


            if (!ad.IsValid)
            {
                Status = DirectoryConnectionStatus.Unconfigured;
                if (FailedConnectionAttempts < 10)
                {
                    FailedConnectionAttempts++;
                }
            }
        }

        private void ConnectDatabase(IDatabaseContext context)
        {
            //We want the latest settings each connection attempt so we make a new database connection

            if (IsCancelRequested)
            {
                return;
            }

            Loggers.ActiveDirectoryLogger.Information("Connecting to settings database");

            //Proceed no further if the DB is down
            if (context.Status != ServiceConnectionState.Up)
            {
                //When cancelling and retrying a connection, the first Up check above is sometimes not Up,
                //but will be one line later. Confirmed with Debugging (3/18/2025)
                //This is the least impactful way and avoids any Task waits, discount double-check
#pragma warning disable S1066 // Mergeable "if" statements should be combined
                if (context.Status != ServiceConnectionState.Up)
                {
                    Status = DirectoryConnectionStatus.UnreachableConfiguration;
                    if (FailedConnectionAttempts < 10)
                    {
                        FailedConnectionAttempts++;
                    }

                    return;
                }
#pragma warning restore S1066 // Mergeable "if" statements should be combined

            }
            Loggers.ActiveDirectoryLogger.Information("Database connected");
        }

        private AppLdapConnection CreateConnection(ADSettings? ad)
        {
            //Perform Auth check
            if (Status != DirectoryConnectionStatus.OK)
            {
                Loggers.ActiveDirectoryLogger.Information("Performing Active Directory connection test");


            }
            var connection = LdapConnectionFactory.Connect(ad);
            if (connection?.LdapConnection == null)
            {
                Loggers.ActiveDirectoryLogger.Information("Active Directory test failed");

                Status = DirectoryConnectionStatus.BadConfiguration;
                if (FailedConnectionAttempts < 10)
                {
                    FailedConnectionAttempts++;
                }

                throw new CriticalActiveDirectoryException(this, "Active Directory test failed");
            }
            Status = DirectoryConnectionStatus.OK;
            return connection;


        }

        private void InitializeDirectoryEntries(ADSettings? ad)
        {
            if (AppRootDirectoryEntry is null || RootDirectoryEntry is null)
            {
                if (ad == null) throw new ArgumentNullException(nameof(ad), "ADSettings cannot be null for initializing directory entries.");
                if (string.IsNullOrEmpty(ad.ApplicationBaseDN)) throw new InvalidOperationException("ApplicationBaseDN is not configured.");
                if (ad.FQDN == null || string.IsNullOrEmpty(ad.FQDN.FqdnToDN())) throw new InvalidOperationException("FQDN is not configured properly to derive root DN.");

                // AppRootDirectoryEntry might be null if ApplicationBaseDN is not set, handle appropriately or ensure it's always set.
                AppRootDirectoryEntry = new LdapDirectoryEntry(ad.ApplicationBaseDN, this);
                Loggers.ActiveDirectoryLogger.Information("App Active Directory context connected using LdapDirectoryEntry for DN: {DN}", ad.ApplicationBaseDN);

                RootDirectoryEntry = new LdapDirectoryEntry(ad.FQDN.FqdnToDN(), this);
                Loggers.ActiveDirectoryLogger.Information("Root Active Directory context connected using LdapDirectoryEntry for DN: {DN}", ad.FQDN.FqdnToDN());
            }
        }

        private void PerformNetworkTests(ADSettings? ad)
        {
            if (ad == null)
            {
                throw new CriticalActiveDirectoryException(this, "Missing configuration");
            }

            Loggers.ActiveDirectoryLogger.Information("Checking Active Directory port status", ad.ServerAddress, ad.ServerPort);

            NetworkTools.ResolveHostIP(ad.ServerAddress);

            if (!NetworkTools.IsPortOpen(ad.ServerAddress, ad.ServerPort))
            {
                Loggers.ActiveDirectoryLogger.Debug("Active Directory port is not open");

                Status = DirectoryConnectionStatus.ServerDown;
                if (FailedConnectionAttempts < 10)
                {
                    FailedConnectionAttempts++;
                }

                throw new CriticalActiveDirectoryException(this, "Active Directory port is not open");

            }
            Loggers.ActiveDirectoryLogger.Information("Active Directory port is open.");
        }

        ///// <summary>
        ///// Tries to get the domain controllers by connecting to the domain from the web server
        ///// </summary>
        ///// <remarks>
        ///// If the web host cannot contact the domain directly via DNS this will not populate <see cref="DomainControllers"/>
        ///// </remarks>
        //private void TryGetDomainControllers()
        //{
        //    try
        //    {
        //        //Clear local list of domain controllers
        //        DomainControllers.Clear();

        //        foreach (DomainController dc in Domain.GetDomain(DirectoryContext).DomainControllers)
        //        {
        //            DomainControllers.Add(dc);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Loggers.ActiveDirectoryLogger.Information("Could not get domain controllers directly {@Error}", ex);
        //    }

        //}

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
            _keepAlive = false;
            _connectionCTS?.Dispose();
            _connectionCTS = null;
            _context?.Dispose();
            _context = null;
        }
        private AuthenticationTypes AuthTypeWin
        {
            get
            {
                AuthenticationTypes _authType = AuthenticationTypes.Secure;
                using var context = Factory.CreateDbContext();
                ADSettings? ad = context?.ActiveDirectorySettings.FirstOrDefault();

                if (ad != null)
                {
                    ConnectionSettings = ad;

                    //We need to determine what security options to use when authenticating
                    //based on the settings in the DB

                    if (ad.UseTLS)
                    {
                        _authType = AuthenticationTypes.Encryption;

                    }
                    if (ad.ServerPort == 636)
                    {
                        _authType = AuthenticationTypes.SecureSocketsLayer | AuthenticationTypes.Secure;

                    }
                }
                return _authType;

            }
        }

        public List<string> DomainControllers { get; private set; } = new();

        public IADUser? Authenticate(LoginRequest loginReq)
        {
            var stopWatch = Stopwatch.StartNew();
            if (loginReq.Username != null && loginReq.Username.Contains('\\'))
            {
                loginReq.Username = loginReq.Username.Substring(loginReq.Username.IndexOf('\\') + 1);
            }
            if (loginReq.Username != null && loginReq.Valid)
            {
                try
                {

                    var findUser = Users.FindUserByUsername(loginReq.Username.ToLower(), true, true);
                    if (findUser != null
                        && ConnectionSettings != null)
                    {

                        try
                        {
                            var authUser = new WindowsImpersonationUser
                            {
                                Username = loginReq.Username,
                                Password = loginReq.SecurePassword,
                                FQDN = ConnectionSettings.FQDN
                            };
                            var authTest = new WindowsImpersonation(authUser);
                            var authResult = authTest.Run(() =>
                            {
                                var impersonatedIdentity = WindowsIdentity.GetCurrent();
                                if (impersonatedIdentity != null &&
                                impersonatedIdentity.IsAuthenticated)
                                {
                                    var impersonatedNameParts = impersonatedIdentity.Name.Split('\\', 2);
                                    if (impersonatedNameParts.Length > 1)
                                    {
                                        var impersonatedName = impersonatedNameParts[1];
                                        if (impersonatedName.Equals(loginReq.Username, StringComparison.InvariantCultureIgnoreCase))
                                        {

                                            return true;
                                        }

                                    }

                                }
                                return false;
                            });
                            stopWatch.Stop();
                            if (authResult)
                            {
                                Loggers.ActiveDirectoryLogger.Debug("Authentication success: {Elapsed} ms", stopWatch.ElapsedMilliseconds);
                                return findUser;
                            }
                            throw new AppException("Local AD Auth Failed");
                        }
                        catch (Exception localAttemptEx)
                        {
                            Loggers.ActiveDirectoryLogger.Information(localAttemptEx, "Local AD auth attempt failed. Attempting remote AD authentication.");


                            if (OperatingSystem.IsLinux())
                            {
                                throw new AppException("AD Auth not implemented");
                            }
                            else
                            {
                                try
                                {
                                    Loggers.ActiveDirectoryLogger.Information("Authenticating Active Directory credentials");

                                    var _authenticatedContext = new DirectoryEntry(LDAP_PROTO + ConnectionSettings.ServerAddress + ":" + ConnectionSettings.ServerPort + "/" + ConnectionSettings.ApplicationBaseDN, loginReq.Username, loginReq.Password, AuthenticationTypes.Secure | AuthenticationTypes.Signing);
                                    _ = _authenticatedContext.AuthenticationType;
                                    var test2 = _authenticatedContext.Children.GetEnumerator();
                                    test2.MoveNext();
                                    var test3 = test2.Current as DirectoryEntry;
                                    _ = test3?.Parent;

                                    _authenticatedContext.Dispose();
                                    stopWatch.Stop();
                                    Loggers.ActiveDirectoryLogger.Debug("Authentication success: {@Elapsed} ms", stopWatch.ElapsedMilliseconds);

                                    return findUser;

                                }
                                catch (DirectoryServicesCOMException ex)
                                {
                                    Loggers.ActiveDirectoryLogger.Information(ex, "Error authenticating user: {@Message}", ex.Message);
                                    if (ex.ExtendedErrorMessage.Contains("data 773, v4563"))
                                    {
                                        Loggers.ActiveDirectoryLogger.Debug("Authentication failure: {@Elapsed} ms", stopWatch.ElapsedMilliseconds);
                                        return null;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    stopWatch.Stop();

                                    Loggers.ActiveDirectoryLogger.Debug("Authentication failure: {Elapsed} ms", stopWatch.ElapsedMilliseconds);

                                    Loggers.ActiveDirectoryLogger.Error(ex, "Error while authenticating credentials.");
                                }
                            }
                            return findUser;
                        }
                    }
                }
                catch (LdapException ex)
                {
                    Loggers.ActiveDirectoryLogger.Debug(ex, "Error authenticating user: " + ex.Message + "");
                    switch (ex.Message)
                    {
                        case "The user name or password is incorrect.":
                            return null;
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// Restores a delete Active Directory entry
        /// </summary>
        /// <param name="model">The entry to be restored</param>
        /// <param name="newOU">The OU to restore to</param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        public bool RestoreTombstone(IDirectoryEntryAdapter model, IADOrganizationalUnit newOU)
        {
            if (!model.IsDeleted)
            {
                throw new AppException(model.CanonicalName + " is not deleted");
            }

            if (ConnectionSettings is null)
            {
                throw new AppException("Active Directory Connection Settings are missing for this enttry");
            }

            string newDN = "CN=" + model.CanonicalName + "," + newOU.DN;

            // 1. Create a modification to remove the 'isDeleted' attribute.
            DirectoryAttributeModification isDeleteAttributeMod = new();
            isDeleteAttributeMod.Name = "isDeleted";
            isDeleteAttributeMod.Operation = DirectoryAttributeOperation.Delete;

            // 2. Create a modification to set the new distinguished name (DN), effectively moving the object.
            DirectoryAttributeModification dnAttributeMod = new();
            dnAttributeMod.Name = ActiveDirectoryFields.DistinguishedName.FieldName;
            dnAttributeMod.Operation = DirectoryAttributeOperation.Replace;
            dnAttributeMod.Add(newDN);

            // Build the request with both modifications.
            var request = new ModifyRequest(model.DN, new DirectoryAttributeModification[] { isDeleteAttributeMod, dnAttributeMod });

            // Add the 'ShowDeletedControl' to allow operations on the "Deleted Objects" container.
            request.Controls.Add(new ShowDeletedControl());

            try
            {

                using (AppLdapConnection connection = LdapConnectionFactory.Connect(ConnectionSettings))
                {
                    var response = (ModifyResponse)connection.SendRequest(request);
                    if (response.ResultCode == ResultCode.Success)
                    {
                        return true;
                    }
                    else
                    {
                        // Log the failure reason from the response for better diagnostics.
                        Loggers.ActiveDirectoryLogger.Warning(
                            "Failed to restore {Name}. AD responded with: {Code} - {Message}",
                            model.CanonicalName, response.ResultCode, response.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                // Preserved exception logging, updated for structured logging.
                Loggers.ActiveDirectoryLogger.Error(ex, "An exception occurred while attempting to restore {Name}", model.CanonicalName);
            }
            return false;

        }

        public IDirectoryEntryAdapter? FindEntryBySID(byte[] sid) => FindEntryBySid(sid.ToSidString());
        public IDirectoryEntryAdapter? FindEntryBySid(string sid)
        {
            var searcher = new ADSearch(this)
            {
                SearchRoot = RootDirectoryEntry
            };
            searcher.Fields.SID = sid;
            var result = searcher.Search().FirstOrDefault();
            return result;
        }
        public IDirectoryEntryAdapter? FindEntryByGuid(string guid) => FindEntryByGuid(Guid.Parse(guid).ToByteArray());

        public IDirectoryEntryAdapter? FindEntryByGuid(byte[] guid)
        {
            if (guid == null)
            {
                return null;
            }

            return new ADSearch(this)
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.Contact,
                Fields = new() { GUID = guid },
                ExactMatch = true

            }.Search<ADContact, IADContact>().FirstOrDefault();
        }

        public IDirectoryEntryAdapter? GetDirectoryEntryByDN(string? dn)
        {
            if (dn == null)
            {
                return null;
            }

            var searcher = new ADSearch(this)
            {
                SearchRoot = RootDirectoryEntry
            };
            searcher.Fields.DN = dn;
            var result = searcher.Search().FirstOrDefault();
            return result;
        }
    }
}
