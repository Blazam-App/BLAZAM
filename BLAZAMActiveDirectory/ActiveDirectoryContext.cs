using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.Common.Data;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Data.Services;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.User;
using BLAZAM.Logger;
using BLAZAM.Notifications.Services;
using BLAZAM.Session.Interfaces;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Security.Claims;
using BLAZAM.Helpers;
using System.DirectoryServices.ActiveDirectory;
using System.Security.Cryptography;

namespace BLAZAM.ActiveDirectory
{
    public class ActiveDirectoryContext : IDisposable, IActiveDirectoryContext
    {
        public IApplicationUserState? CurrentUser
        {
            get
            {
                if (currentUser != null) return currentUser;
                throw new ApplicationException("Current User State was not provided to this directory entry");
            }
            set => currentUser = value;
        }

        private WmiFactory _wmiFactory;
        IEncryptionService _encryption;
        private INotificationPublisher _notificationPublisher;
        public static ActiveDirectoryContext Instance;

        public int FailedConnectionAttempts { get; set; } = 0;

        private AuthenticationTypes _authType;


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
            _authType = AuthenticationTypes.Secure;
            if (ConnectionSettings != null && ConnectionSettings.UseTLS)
            {
                _authType = AuthenticationTypes.SecureSocketsLayer;
            }
            return new DirectoryEntry(
                "LDAP://" + ConnectionSettings?.ServerAddress + ":" + ConnectionSettings?.ServerPort + "/" + baseDN,
                ConnectionSettings?.Username,
                 _encryption.DecryptObject<string>(ConnectionSettings?.Password),
                _authType
                );
        }
        /// <summary>
        /// Gets the root entry for deleted objects in Active Directory
        /// </summary>
        /// <returns></returns>
        public DirectoryEntry GetDeleteObjectsEntry() => new DirectoryEntry("LDAP://" + ConnectionSettings?.ServerAddress + ":" + ConnectionSettings?.ServerPort + "/" + "CN=Deleted Objects," + ConnectionSettings?.FQDN.FqdnToDN(),
                ConnectionSettings?.Username,
                _encryption.DecryptObject<string>(ConnectionSettings?.Password),
                AuthenticationTypes.FastBind | AuthenticationTypes.Secure);

        /// <summary>
        /// Gets all deleted entries from Active Directory
        /// </summary>
        /// <returns></returns>
        public List<IDirectoryEntryAdapter> GetDeletedObjects()
        {
            List<IDirectoryEntryAdapter> found = new List<IDirectoryEntryAdapter>();
            var entry = GetDeleteObjectsEntry();

            DirectorySearcher searcher = new DirectorySearcher(entry);
            searcher.Filter = "(isDeleted=TRUE)";
            searcher.SearchScope = System.DirectoryServices.SearchScope.OneLevel;
            searcher.Tombstone = true;
            var results = searcher.FindAll();

            foreach (SearchResult result in results)
            {
                var model = new DirectoryEntryAdapter();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                model.Parse(result, this);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                found.Add(model);
            }
            return found;
        }

        private Timer _timer;

        /// <summary>

        /// </summary>
        public IADUserSearcher Users { get; }

        /// <summary>

        /// </summary>
        public IADGroupSearcher Groups { get; }

        /// <summary>

        /// </summary>
        public IADOUSearcher OUs { get; }

        /// <summary>

        /// </summary>
        public IADComputerSearcher Computers { get; }

        public IDatabaseContext? Context { get; private set; }

        /// <summary>

        /// </summary>
        public bool PortOpen
        {
            get
            {
                if (ConnectionSettings != null)
                    if (ConnectionSettings.ServerAddress != null && ConnectionSettings.ServerAddress != "")
                        if (ConnectionSettings.ServerPort != 0)
                            return NetworkTools.IsPortOpen(ConnectionSettings.ServerAddress, (int)ConnectionSettings.ServerPort);
                return false;
            }
        }
        private DirectoryConnectionStatus _status = DirectoryConnectionStatus.Connecting;
        private IApplicationUserState? currentUser;

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

        /// <summary>
        /// Called when a new user login matches an Active Directory user
        /// </summary>
        public AppEvent<IApplicationUserState>? OnNewLoginUser { get; set; }

        public IAppDatabaseFactory Factory { get; private set; }

        public ADSettings? ConnectionSettings { get; private set; }

        public IApplicationUserStateService UserStateService { get; set; }

        public WindowsImpersonation Impersonation
        {
            get
            {
                return ConnectionSettings.CreateDirectoryAdminImpersonator();
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
            Instance = this;
            Factory = factory;
            UserStateService = userStateService;
            //UserStateService.UserStateAdded += PopulateUserStateDirectoryUser;
            ConnectAsync();
            _timer = new Timer(KeepAlive, null, 30000, 30000);

            Users = new ADUserSearcher(this);
            Groups = new ADGroupSearcher(this);
            OUs = new ADOUSearcher(this);
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
            Instance = this;
            Factory = activeDirectoryContextSeed.Factory;
            UserStateService = activeDirectoryContextSeed.UserStateService;
            ConnectionSettings = activeDirectoryContextSeed.ConnectionSettings;
            RootDirectoryEntry = activeDirectoryContextSeed.RootDirectoryEntry;
            AppRootDirectoryEntry = activeDirectoryContextSeed.AppRootDirectoryEntry;
            _wmiFactory = activeDirectoryContextSeed._wmiFactory;
            DomainControllers = activeDirectoryContextSeed.DomainControllers;
            Status = activeDirectoryContextSeed.Status;
            // UserStateService.UserStateAdded += PopulateUserStateDirectoryUser;
            //ConnectAsync();
            // _timer = new Timer(KeepAlive, null, 30000, 30000);

            Users = new ADUserSearcher(this);
            Groups = new ADGroupSearcher(this);
            OUs = new ADOUSearcher(this);
            Computers = new ADComputerSearcher(this, activeDirectoryContextSeed._wmiFactory);
        }
        private DirectoryContext DirectoryContext => new DirectoryContext(
            DirectoryContextType.Domain,
            ConnectionSettings.FQDN,
            ConnectionSettings.Username,
            ConnectionSettings.Password.Decrypt()
            );

        public List<DomainController> DomainControllers { get; private set; } = new();


        private async void KeepAlive(object? state)
        {
            if (Status != DirectoryConnectionStatus.OK && Status != DirectoryConnectionStatus.Connecting)
            {
                await ConnectAsync();
            }
            else if (Status == DirectoryConnectionStatus.OK)
            {
                //Throw away query used to keep connection alive
                _ = Users?.FindUsersByString(ConnectionSettings?.Username, false)?.FirstOrDefault();
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
            Loggers.ActiveDirectryLogger.Information("Initiating Active Directory connection");
            try
            {
                //We want the latest settings each connection attempt so we make a new database connection
                //We do this without making a new thread
                //This may be unneccessary
                Context = Factory.CreateDbContext();

                Loggers.ActiveDirectryLogger.Information("Connecting to settings database");

                //Proceed no further if the DB is down
                if (Context.Status == ServiceConnectionState.Up)
                {
                    Loggers.ActiveDirectryLogger.Information("Database connected");
                    //No reason connecting if we're already connected
                    if (Status != DirectoryConnectionStatus.OK)
                    {
                        //Ok get the latest settings
                        ADSettings ad = Context?.ActiveDirectorySettings.FirstOrDefault();
                        ConnectionSettings = ad;

                        if (ad != null)
                        {
                            Loggers.ActiveDirectryLogger.Information("Active Directory settings found in database. {@DirectorySettings}", ad);
                            //We need to determine what security options to use when authenticating
                            //based on the settings in the DB
                            _authType = AuthenticationTypes.Secure;
                            if (ad.UseTLS)
                            {
                                _authType = AuthenticationTypes.Encryption;
                                //_authType = AuthenticationTypes.Secure | AuthenticationTypes.Signing;

                                //_authType = (AuthenticationTypes.SecureSocketsLayer|AuthenticationTypes.Secure);
                            }
                            if (ad.ServerPort == 636)
                            {
                                _authType = AuthenticationTypes.SecureSocketsLayer | AuthenticationTypes.Secure;

                            }

                            if (ad != null && ad.FQDN != null && ad.Username != null)
                            {
                                Loggers.ActiveDirectryLogger.Information("Checking Active Directory port status", ad.ServerAddress, ad.ServerPort);

                                if (NetworkTools.IsPortOpen(ad.ServerAddress, ad.ServerPort))
                                {
                                    Loggers.ActiveDirectryLogger.Information("Active Directory port is open.");

                                    try
                                    {
                                        Loggers.ActiveDirectryLogger.Information("Connecting Active Directory context");
                                        var pass = _encryption.DecryptObject<string>(ad.Password);
                                        AppRootDirectoryEntry = new DirectoryEntry("LDAP://" + ad.ServerAddress + ":" + ad.ServerPort + "/" + ad.ApplicationBaseDN, ad.Username, pass, _authType);
                                        Loggers.ActiveDirectryLogger.Information("App Active Directory context connected");

                                        RootDirectoryEntry = new DirectoryEntry("LDAP://" + ad.ServerAddress + ":" + ad.ServerPort + "/" + ad.FQDN.FqdnToDN(), ad.Username, pass, _authType);

                                        Loggers.ActiveDirectryLogger.Information("Root Active Directory context connected");
                                        pass = null;
                                        //var nativeEntry = DirectoryEntry.NativeObject;
                                        //Perform Auth check
                                        Loggers.ActiveDirectryLogger.Information("Performing Active Directory connection test");

                                        var search = new ADSearch()
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
                                        catch (Exception ex)
                                        {
                                            if (RootDirectoryEntry != null)
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

                                        try
                                        {
                                            if (results.Count > 0)
                                            {
                                                Loggers.ActiveDirectryLogger.Information("Active Directory test passed");

                                                Status = DirectoryConnectionStatus.OK;
                                                DomainControllers.Clear();
                                                foreach (DomainController dc in Domain.GetDomain(DirectoryContext).DomainControllers)
                                                {
                                                    //var test = dc;

                                                    DomainControllers.Add(dc);

                                                }
                                                FailedConnectionAttempts = 0;
                                            }
                                            else
                                            {
                                                Loggers.ActiveDirectryLogger.Warning("Active Directory test failed");

                                                Status = DirectoryConnectionStatus.BadConfiguration;
                                                if (FailedConnectionAttempts < 10)
                                                    FailedConnectionAttempts++;;
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
                                                    Loggers.ActiveDirectryLogger.Error("Error collecting domain controllers {@Error}", ex);
                                                    break;
                                            }
                                        }


                                        return;
                                    }


                                    catch (DirectoryOperationException ex)
                                    {
                                        Loggers.ActiveDirectryLogger.Warning("Error connecting to Active Directory {@Error}", ex);

                                        Status = DirectoryConnectionStatus.BadConfiguration;
                                        if (FailedConnectionAttempts < 10)
                                            FailedConnectionAttempts++; ;
                                        return;
                                    }
                                    catch (CryptographicException ex)
                                    {
                                        Loggers.ActiveDirectryLogger.Warning("Unable to decrypt Active Directory password {@Error}", ex);
                                        Status = DirectoryConnectionStatus.UnreachableConfiguration;
                                        if (FailedConnectionAttempts < 10)
                                            FailedConnectionAttempts++; ;
                                        return;

                                    }
                                    catch (Exception ex)
                                    {
                                        Loggers.ActiveDirectryLogger.Error("Unexpected Error connecting to Active Directory {@Error}", ex);
                                        Status = DirectoryConnectionStatus.BadConfiguration;
                                        if (FailedConnectionAttempts < 10)
                                            FailedConnectionAttempts++; ;
                                        return;

                                    }
                                }
                                else
                                {
                                    Loggers.ActiveDirectryLogger.Warning("Active Directory port is not open");

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
                Loggers.ActiveDirectryLogger.Warning("Unexpected Error connecting to Active Directory {@Error}", ex);

                Status = DirectoryConnectionStatus.ServerDown;
                if (FailedConnectionAttempts < 10)
                    FailedConnectionAttempts++; ;
                return;
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
        /// <summary>
        /// Authenticates a <see cref="LoginRequest"/> to verify
        /// the provided credentials
        /// </summary>
        /// <param name="loginReq"></param>
        /// <returns>The matched user if the credentials are valid, otherwise null.</returns>
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

                    var findUser = Users.FindUserByUsername(loginReq.Username.ToLower(), false);
                    if (findUser != null)
                    {
                        var user = new ADUser();
                        if (ConnectionSettings != null)
                        {
                            if (!loginReq.Username.Contains("@"))
                            {
                                loginReq.Username += "@" + ConnectionSettings.FQDN;
                            }






                            try
                            {
                                Loggers.ActiveDirectryLogger.Information("Authenticating Active Directory credentials");
                                var _authenticatedContext = new DirectoryEntry("LDAP://" + ConnectionSettings.ServerAddress + ":" + ConnectionSettings.ServerPort + "/" + ConnectionSettings.ApplicationBaseDN, loginReq.Username, loginReq.Password, AuthenticationTypes.FastBind);
                                //_authenticatedContext.RefreshCache();
                                var _name = _authenticatedContext.Name;
                                Loggers.ActiveDirectryLogger.Debug("Authentication successful: "+(DateTime.Now-startOfLogon).TotalMilliseconds+"ms");
                                return findUser;

                            }
                            catch (DirectoryServicesCOMException ex)
                            {
                                Loggers.ActiveDirectryLogger.Error("Error authenticating user: " + ex.Message);
                                switch (ex.Message)
                                {
                                    case "The user name or password is incorrect.":
                                        Loggers.ActiveDirectryLogger.Debug("Authentication failure: " + (DateTime.Now - startOfLogon).TotalMilliseconds + "ms");
                                        return null;
                                }
                            }
                            catch (Exception ex)
                            {
                                Loggers.ActiveDirectryLogger.Debug("Authentication failure: " + (DateTime.Now - startOfLogon).TotalMilliseconds + "ms");

                                Loggers.ActiveDirectryLogger.Error("Error while authenticating credentials.", ex);
                            }

                            //WindowsImpersonationUser logonUser = new WindowsImpersonationUser
                            //{
                            //    FQDN = ConnectionSettings.FQDN,
                            //    Username = loginReq.Username,
                            //    Password = loginReq.SecurePassword
                            //};
                            //WindowsImpersonation impersonation = new WindowsImpersonation(logonUser);
                            //try
                            //{
                            //    if (impersonation.Run(() =>
                            //    {
                            //        return true;
                            //    }))
                            //        return findUser;
                            //}
                            //catch (Exception ex)
                            //{

                            //    return null;
                            //}



                            //using (var connection = new LdapConnection(new LdapDirectoryIdentifier(ConnectionSettings.ServerAddress, ConnectionSettings.ServerPort)))
                            //{
                            //    connection.AuthType = AuthType.Basic;
                            //    connection.SessionOptions.ProtocolVersion = 3;
                            //    connection.Timeout = TimeSpan.FromSeconds(5);
                            //    connection.SessionOptions.SecureSocketLayer = ConnectionSettings.UseTLS;
                            //    connection.SessionOptions.PingWaitTimeout = TimeSpan.FromSeconds(5);
                            //    connection.SessionOptions.SendTimeout = TimeSpan.FromSeconds(5);

                            //    //connection.SessionOptions.FastConcurrentBind();
                            //    connection.Credential = new NetworkCredential(loginReq.Username, loginReq.SecurePassword);

                            //    connection.Bind();

                            //    return findUser;

                            //}


                        }
                    }
                }
                catch (LdapException ex)
                {
                    Loggers.ActiveDirectryLogger.Error("Error authenticating user: " + ex.Message);
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
        /// <exception cref="ApplicationException"></exception>
        public bool RestoreTombstone(IDirectoryEntryAdapter model, IADOrganizationalUnit newOU)
        {
            if (!model.IsDeleted) throw new ApplicationException(model.CanonicalName + " is not deleted");
            if (ConnectionSettings is null) throw new ApplicationException("Active Directory Connection Settings are missing for this enttry");
            string newDN = "CN=" + model.CanonicalName + "," + newOU.DN;

            LdapConnection connection = new LdapConnection(
                new LdapDirectoryIdentifier(ConnectionSettings.ServerAddress, ConnectionSettings.ServerPort),
                new NetworkCredential()
                {
                    Domain = ConnectionSettings.FQDN,
                    UserName = ConnectionSettings.Username,
                    SecurePassword = _encryption.DecryptObject<string>(ConnectionSettings.Password).ToSecureString()
                },
                AuthType.Negotiate);

            using (connection)
            {
                string cn = string.Empty;
                connection.Bind();
                connection.SessionOptions.ProtocolVersion = 3;
                DirectoryAttributeModification isDeleteAttributeMod = new DirectoryAttributeModification();
                isDeleteAttributeMod.Name = "isDeleted";
                isDeleteAttributeMod.Operation = DirectoryAttributeOperation.Delete;
                DirectoryAttributeModification dnAttributeMod = new DirectoryAttributeModification();
                dnAttributeMod.Name = "distinguishedName";
                dnAttributeMod.Operation = DirectoryAttributeOperation.Replace;
                dnAttributeMod.Add(newDN);
                ModifyRequest request = new ModifyRequest(model.DN, new DirectoryAttributeModification[] { isDeleteAttributeMod, dnAttributeMod });
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
                    Loggers.ActiveDirectryLogger.Error("Error attempting to restore " + model.CanonicalName);
                    Loggers.ActiveDirectryLogger.Error(ex.Message, ex);
                }
            }
            return false;

        }

        public IDirectoryEntryAdapter? FindEntryBySID(byte[] sid) => GetDirectoryEntryBySid(sid.ToSidString());
        public IDirectoryEntryAdapter? GetDirectoryEntryBySid(string sid)
        {
            var searcher = new ADSearch();
            searcher.SearchRoot = RootDirectoryEntry;
            searcher.Fields.SID = sid;
            var result = searcher.Search().FirstOrDefault();
            return result;
        }
    }
}
