using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Data;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Exceptions;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.User;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using BLAZAM.Notifications.Services;
using BLAZAM.Session.Interfaces;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Security.Cryptography;
using System.Security.Principal;

namespace BLAZAM.ActiveDirectory
{
    public class ActiveDirectoryContext : IDisposable, IActiveDirectoryContext
    {
        public DomainControllerEventLogReader EventLogReader { get; private set; }
        public IApplicationUserState? CurrentUser
        {
            get
            {
                if (_currentUser != null) return _currentUser;
                if (_userStateService.CurrentUserState != null) return _userStateService.CurrentUserState;
                return null;
            }
            set => _currentUser = value;
        }
        private WmiFactory _wmiFactory;
        private IEncryptionService _encryption;
        private INotificationPublisher _notificationPublisher;
        public static ActiveDirectoryContext SystemInstance;

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


        public DirectoryEntry GetDirectoryEntry(string? baseDN = null)
        {
            if (baseDN == null || baseDN == "")
                baseDN = ConnectionSettings?.ApplicationBaseDN;

            return new DirectoryEntry(
                "LDAP://" + ConnectionSettings?.ServerAddress + ":" + ConnectionSettings?.ServerPort + "/" + baseDN,
                ConnectionSettings?.Username,
                 _encryption.DecryptObject<string>(ConnectionSettings?.Password),
                AuthType
                );
        }
        /// <summary>
        /// Gets the root entry for deleted objects in Active Directory
        /// </summary>
        /// <returns></returns>
        public DirectoryEntry GetDeleteObjectsEntry() => new("LDAP://" + ConnectionSettings?.ServerAddress + ":" + ConnectionSettings?.ServerPort + "/" + "CN=Deleted Objects," + ConnectionSettings?.FQDN.FqdnToDN(),
                ConnectionSettings?.Username,
                _encryption.DecryptObject<string>(ConnectionSettings?.Password),
                AuthenticationTypes.FastBind | AuthenticationTypes.Secure);





        public IADUserSearcher Users { get; }

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
        private IApplicationUserState? _currentUser;
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
        public AppEvent<DirectoryConnectionStatus>? OnStatusChanged { get; set; }




        public IAppDatabaseFactory Factory { get; private set; }

        public ADSettings ConnectionSettings { get; private set; }

        private IApplicationUserStateService _userStateService { get; set; }

        public WindowsImpersonation? Impersonation
        {
            get
            {
                return ConnectionSettings?.CreateDirectoryAdminImpersonator();
            }
        }
        /// <summary>
        /// Initializes the applications Active Directory connection. It takes the information
        /// from the ActiveDirectorySetting table in the database and uses them to configure the
        /// connection.
        /// 
        /// </summary>
        /// <param name="context"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public ActiveDirectoryContext(IAppDatabaseFactory factory,
            IApplicationUserStateService userStateService,
            IEncryptionService encryptionService,
            INotificationPublisher notificationPublisher
            )
        {
            _wmiFactory = new(this);
            _encryption = encryptionService;
            _notificationPublisher = notificationPublisher;
            Factory = factory;
            _userStateService = userStateService;
            SystemInstance = this;
            EventLogReader = new(this);
            //UserStateService.UserStateAdded += PopulateUserStateDirectoryUser;
            ConnectAsync();

            Users = new ADUserSearcher(this);
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
            _userStateService = activeDirectoryContextSeed._userStateService;
            ConnectionSettings = activeDirectoryContextSeed.ConnectionSettings;
            RootDirectoryEntry = activeDirectoryContextSeed.RootDirectoryEntry;
            AppRootDirectoryEntry = activeDirectoryContextSeed.AppRootDirectoryEntry;
            _wmiFactory = activeDirectoryContextSeed._wmiFactory;
            DomainControllers = activeDirectoryContextSeed.DomainControllers;
            Status = activeDirectoryContextSeed.Status;
            EventLogReader = activeDirectoryContextSeed.EventLogReader;
            // UserStateService.UserStateAdded += PopulateUserStateDirectoryUser;
            //ConnectAsync();
            // _timer = new Timer(KeepAlive, null, 30000, 30000);

            Users = new ADUserSearcher(this);
            Groups = new ADGroupSearcher(this);
            OUs = new ADOUSearcher(this);
            Printers = new ADPrinterSearcher(this);
            BitLocker = new ADBitLockerSearcher(this);
            Computers = new ADComputerSearcher(this, activeDirectoryContextSeed._wmiFactory);

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
                            Loggers.ActiveDirectoryLogger.Error("Unexpected error performing keep alive search.{@Error}", ex);

                        }
                    }
                    catch (Exception ex)
                    {
                        Loggers.ActiveDirectoryLogger.Error("Unexpected error performing keep alive search.{@Error}", ex);
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
                //We want the latest settings each connection attempt so we make a new database connection
                _context = Factory.CreateDbContext();

                Loggers.ActiveDirectoryLogger.Information("Connecting to settings database");

                //Proceed no further if the DB is down
                if (_context.Status == ServiceConnectionState.Up)
                {
                    Loggers.ActiveDirectoryLogger.Information("Database connected");
                    //No reason connecting if we're already connected
                    if (Status != DirectoryConnectionStatus.OK)
                    {

                        //Ok get the latest settings
                        ADSettings? ad = _context?.ActiveDirectorySettings.FirstOrDefault();

                        if (ad != null)
                        {
                            ConnectionSettings = ad;

                            Loggers.ActiveDirectoryLogger.Information("Active Directory settings found in database. {@DirectorySettings}", ad);
                            //We need to determine what security options to use when authenticating
                            //based on the settings in the DB


                            if (ad != null && ad.FQDN != null && ad.Username != null)
                            {
                                Loggers.ActiveDirectoryLogger.Information("Checking Active Directory port status", ad.ServerAddress, ad.ServerPort);

                                if (NetworkTools.IsPortOpen(ad.ServerAddress, ad.ServerPort))
                                {
                                    Loggers.ActiveDirectoryLogger.Information("Active Directory port is open.");

                                    try
                                    {
                                        Loggers.ActiveDirectoryLogger.Information("Connecting Active Directory context");
                                        var pass = _encryption.DecryptObject<string>(ad.Password);
                                        AppRootDirectoryEntry = new DirectoryEntry("LDAP://" + ad.ServerAddress + ":" + ad.ServerPort + "/" + ad.ApplicationBaseDN, ad.Username, pass, AuthType);
                                        Loggers.ActiveDirectoryLogger.Information("App Active Directory context connected");

                                        RootDirectoryEntry = new DirectoryEntry("LDAP://" + ad.ServerAddress + ":" + ad.ServerPort + "/" + ad.FQDN.FqdnToDN(), ad.Username, pass, AuthType);

                                        Loggers.ActiveDirectoryLogger.Information("Root Active Directory context connected");
                                        pass = null;

                                        //Perform Auth check
                                        Loggers.ActiveDirectoryLogger.Information("Performing Active Directory connection test");

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
                                        try
                                        {
                                            //Check if there is a parent to confirm the app root is a valid OU, even at the root of a domain this reports the domain itself
                                            if (AppRootDirectoryEntry.Parent == null)
                                            {
                                                _notificationPublisher.PublishNotification(new NotificationMessage()
                                                {
                                                    Level = NotificationLevel.Error,
                                                    Message = "The configured BaseDN is not valid. Please correct your settings.",
                                                    Title = "Active Directory Error"
                                                });
                                                Status = DirectoryConnectionStatus.BadConfiguration;
                                                if (FailedConnectionAttempts < 10)
                                                    FailedConnectionAttempts++;
                                                return;
                                            }
                                        }
                                        catch (Exception)
                                        {

                                            Status = DirectoryConnectionStatus.BadConfiguration;
                                            if (FailedConnectionAttempts < 10)
                                                FailedConnectionAttempts++;
                                            return;

                                        }

                                        try
                                        {
                                            if (results.Count > 0)
                                            {
                                                Loggers.ActiveDirectoryLogger.Information("Active Directory test passed");

                                                Status = DirectoryConnectionStatus.OK;
                                                KeepAlive();
                                                TryGetDomainControllers();
                                                FailedConnectionAttempts = 0;
                                            }
                                            else
                                            {
                                                Loggers.ActiveDirectoryLogger.Warning("Active Directory test failed");

                                                Status = DirectoryConnectionStatus.BadConfiguration;
                                                if (FailedConnectionAttempts < 10)
                                                    FailedConnectionAttempts++; ;
                                                return;
                                            }
                                        }
                                        catch (Exception ex)
                                        {

                                            switch (ex.HResult)
                                            {
                                                case -2147016646:
                                                    Status = DirectoryConnectionStatus.EncryptionError;
                                                    break;
                                                case -2147023570:
                                                    Status = DirectoryConnectionStatus.BadCredentials;
                                                    break;
                                                default:
                                                    Loggers.ActiveDirectoryLogger.Warning("Error collecting domain controllers {@Error}", ex);
                                                    break;
                                            }
                                        }


                                        return;
                                    }


                                    catch (DirectoryOperationException ex)
                                    {
                                        Loggers.ActiveDirectoryLogger.Warning("Error connecting to Active Directory {@Error}", ex);

                                        Status = DirectoryConnectionStatus.BadConfiguration;
                                        if (FailedConnectionAttempts < 10)
                                            FailedConnectionAttempts++; ;
                                        return;
                                    }
                                    catch (CryptographicException ex)
                                    {
                                        Loggers.ActiveDirectoryLogger.Warning("Unable to decrypt Active Directory password {@Error}", ex);
                                        Status = DirectoryConnectionStatus.UnreachableConfiguration;
                                        if (FailedConnectionAttempts < 10)
                                            FailedConnectionAttempts++; ;
                                        return;

                                    }
                                    catch (Exception ex)
                                    {
                                        Loggers.ActiveDirectoryLogger.Error("Unexpected Error connecting to Active Directory {@Error}", ex);
                                        Status = DirectoryConnectionStatus.BadConfiguration;
                                        if (FailedConnectionAttempts < 10)
                                            FailedConnectionAttempts++; ;
                                        return;

                                    }
                                }
                                else
                                {
                                    Loggers.ActiveDirectoryLogger.Warning("Active Directory port is not open");

                                    Status = DirectoryConnectionStatus.ServerDown;
                                    if (FailedConnectionAttempts < 10)
                                        FailedConnectionAttempts++; ;
                                    return;
                                }
                            }
                        }
                    }
                }
                Status = DirectoryConnectionStatus.Unconfigured;
                if (FailedConnectionAttempts < 10)
                    FailedConnectionAttempts++; ;
                return;

            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Warning("Unexpected Error connecting to Active Directory {@Error}", ex);

                Status = DirectoryConnectionStatus.ServerDown;
                if (FailedConnectionAttempts < 10)
                    FailedConnectionAttempts++; ;
                return;
            }
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
                Loggers.ActiveDirectoryLogger.Information("Could not get domain controllers directly {@Error}", ex);
            }

        }

        public void Dispose()
        {
            _keepAlive = false;
            _context?.Dispose();
        }

        public IADUser? Authenticate(LoginRequest loginReq)
        {
            var startOfLogon = DateTime.Now;
            if (loginReq.Username != null && loginReq.Username.Contains("\\"))
            {
                loginReq.Username = loginReq.Username.Substring(loginReq.Username.IndexOf("\\") + 1);
            }
            if (loginReq.Username != null && loginReq.Valid)
            {
                try
                {

                    var findUser = Users.FindUserByUsername(loginReq.Username.ToLower(), true, true);
                    if (findUser != null
                        && ConnectionSettings != null)
                    {
                        var username = loginReq.Username;
                        if (!username.Contains("@"))
                        {
                            username += "@" + ConnectionSettings.FQDN;
                        }





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
                                    if (impersonatedNameParts != null && impersonatedNameParts.Length > 1)
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
                            if (authResult == true)
                            {
                                Loggers.ActiveDirectoryLogger.Debug("Authentication success: " + (DateTime.Now - startOfLogon).TotalMilliseconds + "ms");
                                return findUser;
                            }
                            throw new AppException("Local AD Auth Failed");
                        }
                        catch (Exception localAttemptEx)
                        {
                            Loggers.ActiveDirectoryLogger.Warning("Local AD auth attempt failed. Attempting remote AD authentication. {@Error}", localAttemptEx);

                            try
                            {
                                Loggers.ActiveDirectoryLogger.Information("Authenticating Active Directory credentials");

                                var _authenticatedContext = new DirectoryEntry("LDAP://" + ConnectionSettings.ServerAddress + ":" + ConnectionSettings.ServerPort + "/" + ConnectionSettings.ApplicationBaseDN, loginReq.Username, loginReq.Password, AuthType);
                                _ = _authenticatedContext.AuthenticationType;
                                var test2 = _authenticatedContext.Children.GetEnumerator();
                                test2.MoveNext();
                                var test3 = test2.Current as DirectoryEntry;
                                _ = test3?.Parent;

                                _authenticatedContext.Dispose();
                                Loggers.ActiveDirectoryLogger.Debug("Authentication success: " + (DateTime.Now - startOfLogon).TotalMilliseconds + "ms");

                                return findUser;

                            }
                            catch (DirectoryServicesCOMException ex)
                            {
                                Loggers.ActiveDirectoryLogger.Error("Error authenticating user: " + ex.Message + " {@Error}", ex);
                                switch (ex.Message)
                                {
                                    case "The user name or password is incorrect.":
                                        Loggers.ActiveDirectoryLogger.Debug("Authentication failure: " + (DateTime.Now - startOfLogon).TotalMilliseconds + "ms");
                                        return null;
                                }
                            }
                            catch (Exception ex)
                            {
                                Loggers.ActiveDirectoryLogger.Debug("Authentication failure: " + (DateTime.Now - startOfLogon).TotalMilliseconds + "ms");

                                Loggers.ActiveDirectoryLogger.Error("Error while authenticating credentials. {@Error}", ex);
                            }
                        }




                    }

                }
                catch (LdapException ex)
                {
                    Loggers.ActiveDirectoryLogger.Debug("Error authenticating user: " + ex.Message + " {@Error}", ex);
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
                dnAttributeMod.Name = "distinguishedName";
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

        public IDirectoryEntryAdapter? FindEntryBySID(byte[] sid) => GetDirectoryEntryBySid(sid.ToSidString());
        public IDirectoryEntryAdapter? GetDirectoryEntryBySid(string sid)
        {
            var searcher = new ADSearch(this);
            searcher.SearchRoot = RootDirectoryEntry;
            searcher.Fields.SID = sid;
            var result = searcher.Search().FirstOrDefault();
            return result;
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
