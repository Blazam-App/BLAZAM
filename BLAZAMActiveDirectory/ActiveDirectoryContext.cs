using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Data;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Exceptions;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
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
        public DomainControllerEventLogReader EventLogReader { get; private set; }
        public ActiveDirectoryUserState? CurrentUser
        {
            get
            {
                if (_currentUser != null) return _currentUser;
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

        private AuthenticationTypes AuthType
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


        /// <summary>

        /// </summary>
        public DirectoryEntry? AppRootDirectoryEntry { get; private set; }

        /// <summary>
        /// The domain directory entry root
        /// </summary>
        /// <remarks>
        /// Caution should be used when providing this to the UI
        /// </remarks>
        public DirectoryEntry RootDirectoryEntry { get; private set; }


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
            _wmiFactory = new(this);
            _encryption = encryptionService;
            _notificationPublisher = notificationPublisher;
            Factory = factory;
            SetSystemInstance(this);
            EventLogReader = new(this);
            _ = ConnectAsync();

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
            DomainControllers = activeDirectoryContextSeed.DomainControllers;
            Status = activeDirectoryContextSeed.Status;
            EventLogReader = activeDirectoryContextSeed.EventLogReader;

            Users = new ADUserSearcher(this);
            Contacts = new ADContactSearcher(this);
            Groups = new ADGroupSearcher(this);
            OUs = new ADOUSearcher(this);
            Printers = new ADPrinterSearcher(this);
            BitLocker = new ADBitLockerSearcher(this);
            Computers = new ADComputerSearcher(this, activeDirectoryContextSeed._wmiFactory);

        }

        public DirectoryEntry GetDirectoryEntry(string? baseDN = null)
        {
            if (baseDN == null || baseDN == "")
                baseDN = ConnectionSettings?.ApplicationBaseDN;

            return new DirectoryEntry(
                LDAP_PROTO + ConnectionSettings?.ServerAddress + ":" + ConnectionSettings?.ServerPort + "/" + baseDN,
                ConnectionSettings?.Username,
                 _encryption.DecryptObject<string>(ConnectionSettings?.Password),
                AuthType
                );
        }
        /// <summary>
        /// Gets the root entry for deleted objects in Active Directory
        /// </summary>
        /// <returns></returns>
        public DirectoryEntry GetDeleteObjectsEntry() => new(LDAP_PROTO + ConnectionSettings?.ServerAddress + ":" + ConnectionSettings?.ServerPort + "/" + "CN=Deleted Objects," + ConnectionSettings?.FQDN.FqdnToDN(),
                ConnectionSettings?.Username,
                _encryption.DecryptObject<string>(ConnectionSettings?.Password),
                AuthenticationTypes.FastBind | AuthenticationTypes.Secure);





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
                    return NetworkTools.IsPortOpen(ConnectionSettings.ServerAddress, ConnectionSettings.ServerPort);
                return false;
            }
        }
        private DirectoryConnectionStatus _status = DirectoryConnectionStatus.Connecting;
        private ActiveDirectoryUserState? _currentUser;
        private bool _keepAlive;

        public DirectoryConnectionStatus Status
        {
            get => _status; set
            {
                if (_status == value) return;
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

        private DirectoryContext DirectoryContext => new(
            DirectoryContextType.Domain,
            ConnectionSettings.FQDN,
            ConnectionSettings.Username,
            ConnectionSettings.Password.Decrypt()
            );

        public List<DomainController> DomainControllers { get; private set; } = new();


        private async Task KeepAlive()
        {
            if (_systemInstance != this)
            {
                return;
            }

            _keepAlive = true;
            while (_keepAlive)
            {
                await Task.Delay(30000);

                if (Status != DirectoryConnectionStatus.OK && Status != DirectoryConnectionStatus.Connecting)
                {
                    await ConnectAsync();
                }
                else if (Status == DirectoryConnectionStatus.OK)
                {
                    //Throw away query used to keep connection alive
                    try
                    {
                        _ = (await Users.FindUsersByStringAsync(ConnectionSettings?.Username, false))?.FirstOrDefault();

                    }
                    catch (DirectoryServicesCOMException ex)
                    {
                        //not usernam or password is incorrect
                        if (ex.HResult != -2147023570)
                        {
                            Loggers.ActiveDirectoryLogger.Error(ex, "Unexpected error performing keep alive search.");

                        }
                    }
                    catch (Exception ex)
                    {
                        Loggers.ActiveDirectoryLogger.Error(ex, "Unexpected error performing keep alive search.");
                    }
                }
            }
        }



        public async Task ConnectAsync()
        {
            Status = DirectoryConnectionStatus.Connecting;
            await Task.Run(() =>
            {
                Connect();

            });

        }
        public async Task CancelConnection()
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
        public void Connect()
        {

            //Set status flag
            Status = DirectoryConnectionStatus.Connecting;

            Loggers.ActiveDirectoryLogger.Information("Initiating Active Directory connection");
            try
            {
                ConnectDatabase();

                if (IsCancelRequested) return;

                ADSettings? ad;

                GetConnectionSettings(out ad);

                if (IsCancelRequested) return;

                PerformNetworkTests(ad);


                if (IsCancelRequested) return;

                InitializeDirectoryEntries(ad);

                if (IsCancelRequested) return;

                PerformConnectionTests(ad);

            }
            catch (UnresolvableAddressException ex)
            {
                ConnectionException = ex;

                Loggers.ActiveDirectoryLogger.Warning(ex, "Unable to resolve Active Directory server address");
                Status = DirectoryConnectionStatus.ServerDown;
                if (FailedConnectionAttempts < 10)
                    FailedConnectionAttempts++;
            }
            catch (DirectoryOperationException ex)
            {
                ConnectionException = ex;

                Loggers.ActiveDirectoryLogger.Warning(ex, "Error connecting to Active Directory");

                Status = DirectoryConnectionStatus.BadConfiguration;
                if (FailedConnectionAttempts < 10)
                    FailedConnectionAttempts++;
            }
            catch (CryptographicException ex)
            {
                ConnectionException = ex;

                Loggers.ActiveDirectoryLogger.Warning(ex, "Unable to decrypt Active Directory password");
                Status = DirectoryConnectionStatus.EncryptionError;
                if (FailedConnectionAttempts < 10)
                    FailedConnectionAttempts++;

            }
            catch (DirectoryServicesCOMException ex)
            {
                ConnectionException = ex;
                switch (ex.ExtendedError)
                {
                    case -2146893044:
                        Loggers.ActiveDirectoryLogger.Information(ex, "Bad credentials for Active Directory");

                        Status = DirectoryConnectionStatus.BadCredentials;
                        break;

                    case 8235:
                        Loggers.ActiveDirectoryLogger.Information(ex, "Bad configuration for Active Directory");

                        Status = DirectoryConnectionStatus.BadConfiguration;
                        break;
                    case 8333:
                        Loggers.ActiveDirectoryLogger.Information(ex, "RootOU container not found in Active Directory");

                        Status = DirectoryConnectionStatus.ContainerNotFound;
                        break;
                    default:
                        Loggers.ActiveDirectoryLogger.Warning(ex, "Unexpected Error connecting to Active Directory");
                        Status = DirectoryConnectionStatus.ServerDown;
                        break;
                }
                if (FailedConnectionAttempts < 10)
                    FailedConnectionAttempts++;
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
                    FailedConnectionAttempts++;
            }
            catch (CriticalActiveDirectoryException ex)
            {
                ConnectionException = ex;

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
                    FailedConnectionAttempts++;
            }
            finally
            {
                if (IsCancelRequested == false && Status != DirectoryConnectionStatus.OK)
                {
                    Task.Delay(5000).Wait();
                    Connect();
                }
            }
        }
        private bool IsCancelRequested
        {
            get
            {
                return _connectionCTS != null && _connectionCTS.IsCancellationRequested;
            }
        }
        private void GetConnectionSettings(out ADSettings? ad)
        {
            //Ok get the latest settings
            ad = _context?.ActiveDirectorySettings.FirstOrDefault();
            if (IsCancelRequested) return;

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
                    FailedConnectionAttempts++;
            }
        }

        private void ConnectDatabase()
        {
            //We want the latest settings each connection attempt so we make a new database connection
            _context = Factory.CreateDbContext();

            if (IsCancelRequested) return;

            Loggers.ActiveDirectoryLogger.Information("Connecting to settings database");

            //Proceed no further if the DB is down
            if (_context.Status != ServiceConnectionState.Up)
            {
                //When cancelling and retrying a connection, the first Up check above is sometimes not Up,
                //but will be one line later. Confirmed with Debugging (3/18/2025)
                //This is the least impactful way and avoids any Task waits, discount double-check
#pragma warning disable S1066 // Mergeable "if" statements should be combined
                if (_context.Status != ServiceConnectionState.Up)
                {
                    Status = DirectoryConnectionStatus.UnreachableConfiguration;
                    if (FailedConnectionAttempts < 10)
                        FailedConnectionAttempts++;
                    return;
                }
#pragma warning restore S1066 // Mergeable "if" statements should be combined

            }
            Loggers.ActiveDirectoryLogger.Information("Database connected");
        }

        private void PerformConnectionTests(ADSettings? ad)
        {
            //Perform Auth check
            Loggers.ActiveDirectoryLogger.Information("Performing Active Directory connection test");

            _ = RootDirectoryEntry.Name;
            _ = AppRootDirectoryEntry?.Name;


            var search = new ADSearch(this)
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.User,
                SearchRoot = RootDirectoryEntry,
                Fields = new()
                {
                    SamAccountName = ad.Username
                },
                ExactMatch = true
            };
            var results = search.Search<ADUser, IADUser>();


            if (results.Count > 0)
            {
                Loggers.ActiveDirectoryLogger.Information("Active Directory test passed");
                ConnectionException = null;

                Status = DirectoryConnectionStatus.OK;
                KeepAlive();
                TryGetDomainControllers();
                FailedConnectionAttempts = 0;
                return;

            }
            else
            {
                Loggers.ActiveDirectoryLogger.Warning("Active Directory test failed");

                Status = DirectoryConnectionStatus.BadConfiguration;
                if (FailedConnectionAttempts < 10)
                    FailedConnectionAttempts++;
                throw new CriticalActiveDirectoryException(this, "Active Directory test failed");

            }
        }

        private void InitializeDirectoryEntries(ADSettings? ad)
        {
            var pass = _encryption.DecryptObject<string>(ad.Password);

            AppRootDirectoryEntry = new DirectoryEntry(
                LDAP_PROTO + ad.ServerAddress + ":" + ad.ServerPort + "/" + ad.ApplicationBaseDN,
                ad.Username,
                pass,
                AuthType);
            Loggers.ActiveDirectoryLogger.Information("App Active Directory context connected");

            RootDirectoryEntry = new DirectoryEntry(
                LDAP_PROTO + ad.ServerAddress + ":" + ad.ServerPort + "/" + ad.FQDN.FqdnToDN(),
                ad.Username,
                pass,
                AuthType);

            Loggers.ActiveDirectoryLogger.Information("Root Active Directory context connected");
        }

        private void PerformNetworkTests(ADSettings? ad)
        {
            if (ad == null) throw new CriticalActiveDirectoryException(this, "Missing configuration");

            Loggers.ActiveDirectoryLogger.Information("Checking Active Directory port status", ad.ServerAddress, ad.ServerPort);

            NetworkTools.ResolveHostIP(ad.ServerAddress);

            if (!NetworkTools.IsPortOpen(ad.ServerAddress, ad.ServerPort))
            {
                Loggers.ActiveDirectoryLogger.Debug("Active Directory port is not open");

                Status = DirectoryConnectionStatus.ServerDown;
                if (FailedConnectionAttempts < 10)
                    FailedConnectionAttempts++;
                throw new CriticalActiveDirectoryException(this, "Active Directory port is not open");

            }
            Loggers.ActiveDirectoryLogger.Information("Active Directory port is open.");
        }


        /// <summary>
        /// Tries to get the domain controllers by connecting to the domain from the web server
        /// </summary>
        /// <remarks>
        /// If the web host cannot contact the domain directly via DNS this will not populate <see cref="DomainControllers"/>
        /// </remarks>
        private void TryGetDomainControllers()
        {
            try
            {
                //Clear local list of domain controllers
                DomainControllers.Clear();

                foreach (DomainController dc in Domain.GetDomain(DirectoryContext).DomainControllers)
                {
                    DomainControllers.Add(dc);
                }
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Information(ex, "Could not get domain controllers directly");
            }

        }

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

                            try
                            {
                                Loggers.ActiveDirectoryLogger.Information("Authenticating Active Directory credentials");

                                var _authenticatedContext = new DirectoryEntry(LDAP_PROTO + ConnectionSettings.ServerAddress + ":" + ConnectionSettings.ServerPort + "/" + ConnectionSettings.ApplicationBaseDN, loginReq.Username, loginReq.Password, AuthType);
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
                                    return findUser;
                                }
                                switch (ex.Message)
                                {
                                    case "The user name or password is incorrect.":
                                        stopWatch.Stop();

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
            if (!model.IsDeleted) throw new AppException(model.CanonicalName + " is not deleted");
            if (ConnectionSettings is null) throw new AppException("Active Directory Connection Settings are missing for this enttry");
            string newDN = "CN=" + model.CanonicalName + "," + newOU.DN;

            LdapConnection connection = new(
                new LdapDirectoryIdentifier(ConnectionSettings.ServerAddress, ConnectionSettings.ServerPort),
                new NetworkCredential()
                {
                    Domain = ConnectionSettings.FQDN,
                    UserName = ConnectionSettings.Username,
                    SecurePassword = _encryption.DecryptObject<string>(ConnectionSettings.Password)?.ToSecureString()
                },
                System.DirectoryServices.Protocols.AuthType.Negotiate);

            using (connection)
            {
                connection.Bind();
                connection.SessionOptions.ProtocolVersion = 3;
                DirectoryAttributeModification isDeleteAttributeMod = new();
                isDeleteAttributeMod.Name = "isDeleted";
                isDeleteAttributeMod.Operation = DirectoryAttributeOperation.Delete;
                DirectoryAttributeModification dnAttributeMod = new();
                dnAttributeMod.Name = ActiveDirectoryFields.DistinguishedName.FieldName;
                dnAttributeMod.Operation = DirectoryAttributeOperation.Replace;
                dnAttributeMod.Add(newDN);
                ModifyRequest request = new(model.DN, new DirectoryAttributeModification[] { isDeleteAttributeMod, dnAttributeMod });
                request.Controls.Add(new ShowDeletedControl());

                try
                {
                    ModifyResponse response = (ModifyResponse)connection.SendRequest(request);
                    if (response.ResultCode == ResultCode.Success)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectoryLogger.Error("Error attempting to restore " + model.CanonicalName + "{@Error}", ex);
                }
            }
            return false;

        }

        public IDirectoryEntryAdapter? FindEntryBySID(byte[] sid) => FindEntryBySid(sid.ToSidString());
        public IDirectoryEntryAdapter? FindEntryBySid(string sid)
        {
            var searcher = new ADSearch(this);
            searcher.SearchRoot = RootDirectoryEntry;
            searcher.Fields.SID = sid;
            var result = searcher.Search().FirstOrDefault();
            return result;
        }
        public IDirectoryEntryAdapter? FindEntryByGuid(string guid) => FindEntryByGuid(Guid.Parse(guid).ToByteArray());

        public IDirectoryEntryAdapter? FindEntryByGuid(byte[] guid)
        {
            if (guid == null) return null;
            return new ADSearch(this)
            {
                ObjectTypeFilter = ActiveDirectoryObjectType.Contact,
                Fields = new() { GUID = guid },
                ExactMatch = true

            }.Search<ADContact, IADContact>().FirstOrDefault();
        }

        public IDirectoryEntryAdapter? GetDirectoryEntryByDN(string? dn)
        {
            if (dn == null) return null;
            var searcher = new ADSearch(this);
            searcher.SearchRoot = RootDirectoryEntry;
            searcher.Fields.DN = dn;
            var result = searcher.Search().FirstOrDefault();
            return result;
        }
    }
}
