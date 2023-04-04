using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.ActiveDirectory.Models;
using BLAZAM.Common.Data.ActiveDirectory.Searchers;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Extensions;
using BLAZAM.Common.Helpers;
using BLAZAM.Common.Models.Database;
using BLAZAM.Common.Models.Database.User;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Security.Claims;

namespace BLAZAM.Common.Data.ActiveDirectory
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

        private WmiFactoryService _wmiFactory;
        IEncryptionService _encryption;
        private INotificationPublisher _notificationPublisher;
        public static ActiveDirectoryContext Instance;


        private AuthenticationTypes _authType;


        /// <summary>
        /// <inheritdoc/>
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
                (AuthenticationTypes.FastBind | AuthenticationTypes.Secure));

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
        /// <inheritdoc/>
        /// </summary>
        public IADUserSearcher Users { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IADGroupSearcher Groups { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IADOUSearcher OUs { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IADComputerSearcher Computers { get; }

        public IDatabaseContext? Context { get; private set; }

        /// <summary>
        /// <inheritdoc/>
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DirectoryConnectionStatus Status
        {
            get => _status; set
            {
                if (_status == value) return;
                _status = value;
                OnStatusChanged?.Invoke(_status);
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public AppEvent<DirectoryConnectionStatus>? OnStatusChanged { get; set; }

        /// <summary>
        /// Called when a new user login matches an Active Directory user
        /// </summary>
        public AppEvent<IApplicationUserState>? OnNewLoginUser { get; set; }

        public AppDatabaseFactory? Factory { get; private set; }

        public ADSettings? ConnectionSettings { get; private set; }

        public IApplicationUserStateService UserStateService { get; set; }

        /// <summary>
        /// Initializes the applications Active Directory connection. It takes the information
        /// from the ActiveDirectorySetting table in the database and uses them to configure the
        /// connection.
        /// 
        /// Upon creation, no actual connection attempt has been made yet, to verify the connection
        /// status, check the Status property.
        /// </summary>
        /// <param name="context"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public ActiveDirectoryContext(AppDatabaseFactory factory,
            IApplicationUserStateService userStateService,
            WmiFactoryService wmiFactory,
            IEncryptionService encryptionService,
            INotificationPublisher notificationPublisher
            )
        {
            _wmiFactory = wmiFactory;
            _encryption = encryptionService;
            _notificationPublisher = notificationPublisher;
            Instance = this;
            Factory = factory;
            UserStateService = userStateService;
            UserStateService.UserStateAdded += PopulateUserStateDirectoryUser;
            ConnectAsync();
            _timer = new Timer(KeepAlive, null, 30000, 30000);

            Users = new ADUserSearcher(this);
            Groups = new ADGroupSearcher(this);
            OUs = new ADOUSearcher(this);
            Computers = new ADComputerSearcher(this, wmiFactory);
        }

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

            // UserStateService.UserStateAdded += PopulateUserStateDirectoryUser;
            ConnectAsync();
            // _timer = new Timer(KeepAlive, null, 30000, 30000);

            Users = new ADUserSearcher(this);
            Groups = new ADGroupSearcher(this);
            OUs = new ADOUSearcher(this);
            Computers = new ADComputerSearcher(this, activeDirectoryContextSeed._wmiFactory);
        }

        private void PopulateUserStateDirectoryUser(IApplicationUserState value)
        {
            if (value != null && value.User != null & value.User?.Identity?.AuthenticationType == AppAuthenticationTypes.ActiveDirectoryAuthentication && value.DirectoryUser == null)
            {
                value.DirectoryUser = Users.FindUserBySID(value.User.FindFirstValue(ClaimTypes.Sid));
                OnNewLoginUser?.Invoke(value);
            }
        }

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
            Status = DirectoryConnectionStatus.Connecting;

            try
            {
                Context = Factory.CreateDbContext();


                if (Context.Status == ServiceConnectionState.Up)
                {
                    if (Status != DirectoryConnectionStatus.OK)
                    {
                        ADSettings ad = Context?.ActiveDirectorySettings.FirstOrDefault();
                        ConnectionSettings = ad;

                        if (ad != null)
                        {
                            //_authType = AuthenticationTypes.Secure;
                            _authType = AuthenticationTypes.Secure | AuthenticationTypes.Signing;
                            if (ad.UseTLS)
                            {
                                //_authType = AuthenticationTypes.Encryption;
                                _authType = (AuthenticationTypes.Encryption | AuthenticationTypes.Secure);

                                //_authType = (AuthenticationTypes.SecureSocketsLayer|AuthenticationTypes.Secure);
                            }

                            if (ad != null && ad.FQDN != null && ad.Username != null)
                            {
                                if (NetworkTools.IsPortOpen(ad.ServerAddress, ad.ServerPort))
                                {
                                    try
                                    {

                                        AppRootDirectoryEntry = new DirectoryEntry("LDAP://" + ad.ServerAddress + ":" + ad.ServerPort + "/" + ad.ApplicationBaseDN, ad.Username, _encryption.DecryptObject<string>(ad.Password), _authType);
                                        RootDirectoryEntry = new DirectoryEntry("LDAP://" + ad.ServerAddress + ":" + ad.ServerPort + "/" + ad.FQDN.FqdnToDN(), ad.Username, _encryption.DecryptObject<string>(ad.Password), _authType);
                                        //var nativeEntry = DirectoryEntry.NativeObject;
                                        //Perform Auth check
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
                                            if (AppRootDirectoryEntry.Parent == null)
                                            {
                                                _notificationPublisher.PublishNotification(new NotificationMessage()
                                                {
                                                    Level = NotificationLevel.Error,
                                                    Message = "The configured BaseDN is not valid. Please correct your settings.",
                                                    Title = "Active Directory Error"
                                                });
                                                Status = DirectoryConnectionStatus.BadConfiguration; return;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            _notificationPublisher.PublishNotification(new NotificationMessage()
                                            {
                                                Level = NotificationLevel.Error,
                                                Message = "The configured BaseDN is not valid. Please correct your settings.",
                                                Title = "Active Directory Error"
                                            });
                                            Status = DirectoryConnectionStatus.BadConfiguration; return;

                                        }

                                        try
                                        {
                                            if (results.Count > 0)
                                                Status = DirectoryConnectionStatus.OK;
                                            else
                                                Status = DirectoryConnectionStatus.BadConfiguration;
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
                                            }
                                        }


                                        return;
                                    }


                                    catch (DirectoryOperationException)
                                    {
                                        Status = DirectoryConnectionStatus.BadConfiguration;
                                    }
                                }
                                else
                                {
                                    Status = DirectoryConnectionStatus.ServerDown;
                                }
                            }
                        }
                    }
                }
                Status = DirectoryConnectionStatus.Unconfigured;


            }
            catch (Exception)
            {
                Status = DirectoryConnectionStatus.ServerDown;

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
            if (loginReq.Username.Contains("\\"))
            {
                loginReq.Username = loginReq.Username.Substring(loginReq.Username.IndexOf("\\") + 1);
            }
            if (loginReq.Valid)
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
                            using (var connection = new LdapConnection(new LdapDirectoryIdentifier(ConnectionSettings.ServerAddress, ConnectionSettings.ServerPort)))
                            {
                                connection.AuthType = AuthType.Basic;
                                connection.SessionOptions.ProtocolVersion = 3;
                                connection.SessionOptions.SecureSocketLayer = ConnectionSettings.UseTLS;

                                connection.Credential = new NetworkCredential(loginReq.Username, loginReq.SecurePassword);
                                connection.Bind();

                                return findUser;

                            }

                        }
                    }
                }
                catch (LdapException ex)
                {
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
                    if (response.ResultCode != ResultCode.Success)
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

        public IDirectoryEntryAdapter? GetDirectoryModelBySid(byte[] sid) => GetDirectoryModelBySid(sid.ToSidString());
        public IDirectoryEntryAdapter? GetDirectoryModelBySid(string sid)
        {
            var searcher = new ADSearch();
            searcher.SearchRoot = RootDirectoryEntry;
            searcher.Fields.SID = sid;
            var result = searcher.Search().FirstOrDefault();
            return result;
        }
    }
}
