using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.ActiveDirectory.Models;
using BLAZAM.Common.Data.ActiveDirectory.Searchers;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Helpers;
using BLAZAM.Common.Models.Database;
using Microsoft.EntityFrameworkCore;
using System.Buffers.Text;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Claims;

namespace BLAZAM.Common.Data.ActiveDirectory
{
    public class ActiveDirectoryContext : IDisposable, IActiveDirectory
    {

        public static ActiveDirectoryContext Instance;

        IADUser? _keepAliveUser { get; set; }

        private AuthenticationTypes _authType;
        public DirectoryEntry? DirectoryEntry { get; private set; }
        public DirectoryEntry GetDirectoryEntry(string? baseDN = null)
        {
            if (baseDN == null || baseDN == "")
                baseDN = ConnectionSettings?.ApplicationBaseDN;
            _authType = AuthenticationTypes.Secure;
            if (ConnectionSettings != null && ConnectionSettings.UseTLS)
            {
                _authType = AuthenticationTypes.SecureSocketsLayer;
            }
            return DirectoryEntry = new DirectoryEntry(
                "LDAP://" + ConnectionSettings?.ServerAddress + ":" + ConnectionSettings?.ServerPort + "/" + baseDN,
                ConnectionSettings?.Username,
                ConnectionSettings?.Password,
                _authType
                );
        }
        public DirectoryEntry GetDeleteObjectsEntry() => new DirectoryEntry("LDAP://" + ConnectionSettings?.ServerAddress + ":" + ConnectionSettings?.ServerPort + "/" + "CN=Deleted Objects," + ConnectionSettings?.ApplicationBaseDN,
                ConnectionSettings?.Username,
                ConnectionSettings?.Password,
                (AuthenticationTypes.FastBind | AuthenticationTypes.Secure));

        public List<IDirectoryModel> GetDeletedObjects()
        {
            List<IDirectoryModel> found = new List<IDirectoryModel>();
            var entry = GetDeleteObjectsEntry();

            DirectorySearcher searcher = new DirectorySearcher(entry);
            searcher.Filter = "(isDeleted=TRUE)";
            searcher.SearchScope = System.DirectoryServices.SearchScope.OneLevel;
            searcher.Tombstone = true;
            var results = searcher.FindAll();

            foreach (SearchResult result in results)
            {
                var model = new DirectoryModel();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                model.Parse(result, this);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                found.Add(model);
            }
            return found;
        }
        private Timer t;

        public IADUserSearcher Users { get; }
        public IADGroupSearcher Groups { get; }
        public IADOUSearcher OUs { get; }
        public IADComputerSearcher Computers { get; }

        public DatabaseContext? Context { get; private set; }
        public bool Pingable
        {
            get
            {
                if (ConnectionSettings != null)

                    if (ConnectionSettings.ServerAddress != null && ConnectionSettings.ServerAddress != "")
                        return NetworkTools.PingHost(ConnectionSettings.ServerAddress);
                return false;
            }
        }
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
        public AppEvent<IApplicationUserState>? OnNewLoginUser { get; set; }
        public IDbContextFactory<DatabaseContext>? Factory { get; private set; }
        public ADSettings? ConnectionSettings { get; private set; }

        public IApplicationUserStateService? UserStateService { get; set; }

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
        public ActiveDirectoryContext(IDbContextFactory<DatabaseContext> factory,
            IApplicationUserStateService userStateService,
            WmiFactoryService wmiFactory)
        {
            Instance = this;
            Factory = factory;
            UserStateService = userStateService;
            UserStateService.UserStateAdded += PopulateUserStateDirectoryUser;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ConnectAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            t = new Timer(KeepAlive, null, 30000, 30000);

            Users = new ADUserSearcher(this);
            Groups = new ADGroupSearcher(this);
            OUs = new ADOUSearcher(this);
            Computers = new ADComputerSearcher(this, wmiFactory);
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
            _keepAliveUser = null;
            if (Status != DirectoryConnectionStatus.OK && Status != DirectoryConnectionStatus.Connecting)
            {
                await ConnectAsync();
            }
            else if (Status == DirectoryConnectionStatus.OK)
            {
                //Throw away query used to keep connection alive
                _keepAliveUser = Users?.FindUsersByString(ConnectionSettings?.Username, false)?.FirstOrDefault();
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

        public void Connect()
        {
            Status = DirectoryConnectionStatus.Connecting;

            try
            {
                Context = Factory.CreateDbContext();


                if (Context.Status == DatabaseContext.ConnectionStatus.OK)
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
                                if (NetworkTools.PingHost(ad.ServerAddress))
                                {
                                    try
                                    {

                                        DirectoryEntry = new DirectoryEntry("LDAP://" + ad.ServerAddress + ":" + ad.ServerPort + "/" + ad.ApplicationBaseDN, ad.Username, ad.Password, _authType);
                                        //var nativeEntry = DirectoryEntry.NativeObject;
                                        using (var authTest = new DirectorySearcher(DirectoryEntry, "(sAMAccountName=" + ad.Username + ")"))
                                        {
                                            try
                                            {
                                                var result = Users.FindUsersByString(ad.Username);
                                                if (result.Count > 0)
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
                            else
                                Status = DirectoryConnectionStatus.Unconfigured;
                        }
                        else
                            Status = DirectoryConnectionStatus.Unconfigured;
                    }
                }
                else
                {
                    Status = DirectoryConnectionStatus.Unconfigured;

                }
            }
            catch (Exception)
            {
                Status = DirectoryConnectionStatus.ServerDown;

            }
        }

        public void Dispose()
        {
            t.Dispose();
        }

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

        public bool RestoreTombstone(IDirectoryModel model, IADOrganizationalUnit newOU)
        {
            if (!model.IsDeleted) throw new ApplicationException(model.CanonicalName + " is not deleted");

            string newDN = "CN=" + model.CanonicalName + "," + newOU.DN;

            LdapConnection connection = new LdapConnection(
                new LdapDirectoryIdentifier(ConnectionSettings.ServerAddress, ConnectionSettings.ServerPort),
                new NetworkCredential()
                {
                    Domain = ConnectionSettings.FQDN,
                    UserName = ConnectionSettings.Username,
                    SecurePassword = ConnectionSettings.Password.ToSecureString()
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
                catch (Exception exception)
                {
                }
            }
            return false;

        }
    }
}
