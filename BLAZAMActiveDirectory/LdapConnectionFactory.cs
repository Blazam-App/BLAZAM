using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Net;
using BLAZAM.ActiveDirectory.Data;
using BLAZAM.Common.Exceptions;
using BLAZAM.Database.Models;
using BLAZAM.Helpers;
using BLAZAM.Logger;

namespace BLAZAM.ActiveDirectory
{
    public class LdapConnectionFactory : IDisposable
    {
        public static int PoolSize => (5 * _connectedUsers);
        private static int _connectedUsers { get; set; }
        private Timer? _disposerTimer = null;
        private static readonly object _poolLock = new object();
        private static ADSettings? _connectionSettingsCache = null;
        private readonly List<AppLdapConnection> _connectionPool = new();
        private static Random _random;
        private bool disposedValue;
        private static readonly object _tlsLock = new();

 
        public int Count
        {
            get
            {
                lock (_poolLock)
                {
                    return _connectionPool.Count;
                }
            }
        }
        public static AppEvent? OnCountChanged { get; set; } = new();

        /// <summary>
        /// Establishes a secure LDAP connection based on ADSettings.
        /// It will choose LDAPS if port is typically 636 and UseTLS is true,
        /// or StartTLS if port is typically 389 and UseTLS is true.
        /// </summary>
        /// <param name="settings">The ADSettings object containing connection parameters.</param>
        /// <param name="connection">The established LdapConnection object if successful, otherwise null.</param>
        /// <returns>True if the connection was successful, otherwise false.</returns>
        public async Task<AppLdapConnection?> ConnectAsync(ADSettings settings)
        {

            return await Task.Run(() => Connect(settings));

        }
        /// <summary>
        /// Establishes a secure LDAP connection based on ADSettings.
        /// It will choose LDAPS if port is typically 636 and UseTLS is true,
        /// or StartTLS if port is typically 389 and UseTLS is true.
        /// </summary>
        /// <param name="settings">The ADSettings object containing connection parameters.</param>
        /// <param name="connection">The established LdapConnection object if successful, otherwise null.</param>
        /// <returns>True if the connection was successful, otherwise false.</returns>
        public AppLdapConnection? Connect(ADSettings settings, string? serverHostname = null)
        {
            bool startedTLS = false;
            if (_random == null)
            {
                _random = new Random();
            }
            var delay = _random.Next(0, 50);
            Task.Delay(delay).Wait();
            LdapConnection connection = null;
            if (settings == null)
            {
                Loggers.ActiveDirectoryLogger.Information("ADSettings object is null.");
                return default;
            }
            _connectionSettingsCache = settings;
            // Optional: Check the IsValid property from ADSettings, though the individual Connect methods will also fail if parameters are bad.
            // if (!settings.IsValid)
            // {
            //     Console.WriteLine("ADSettings are invalid according to its IsValid property.");
            //     return false;
            // }

            if (string.IsNullOrEmpty(settings.ServerAddress))
            {
                Loggers.ActiveDirectoryLogger.Information("ServerAddress in ADSettings is null or empty.");
                return default;
            }
            if (string.IsNullOrEmpty(settings.Username))
            {
                Loggers.ActiveDirectoryLogger.Information("Username in ADSettings is null or empty.");
                return default;
            }
            if (string.IsNullOrEmpty(settings.Password))
            {
                Loggers.ActiveDirectoryLogger.Information("Password in ADSettings is null or empty.");
                return default;
            }

            lock (_poolLock)
            {
                AppLdapConnection? conn = null;
                try
                {
                    if (serverHostname.IsNullOrEmpty())
                    {
                        conn = _connectionPool.First(c => !c.IsDisposed && c.Expires != null);
                        if (conn != null && conn.LdapConnection != null)
                        {
                            var whoAmIRequest = new SearchRequest("", "(objectClass=*)", SearchScope.Base, settings.ApplicationBaseDN);
                            conn.LdapConnection.SendRequest(whoAmIRequest);
                            conn.Expires = null;
                            return conn;
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    //Ignore no sequence elements error after filtering pool
                }
                catch (Exception)
                {
                    // The connection is dead. Dispose of it permanently and try the next one.
                    if (conn != null)
                    {
                        conn.DisposeNow();
                        _connectionPool.Remove(conn);
                        OnCountChanged?.Invoke();
                    }
                }



                // Typically, port 636 is for LDAPS, and 389 is for LDAP (which can be upgraded with StartTLS).
                // We'll infer the method based on common port usage when UseTLS is true.
                if (settings.ServerPort == 636) // Common LDAPS port
                {
                    Loggers.ActiveDirectoryLogger.Information($"ADSettings: UseTLS is true, port is {settings.ServerPort}. Attempting LDAPS connection.");
                    ConnectWithLdaps(settings, out connection,serverHostname);
                }
                else if (settings.ServerPort == 389) // Common LDAP port, suitable for StartTLS
                {
                    Loggers.ActiveDirectoryLogger.Information($"ADSettings: UseTLS is true, port is {settings.ServerPort}. Attempting StartTLS connection.");
                    ConnectWithStartTls(settings, out connection, serverHostname);
                    startedTLS = true;
                }
                else
                {
                    // If UseTLS is true but port is neither 389 nor 636, it's ambiguous.
                    // For this example, we'll try LDAPS as a default secure method if UseTLS is true and port is non-standard.
                    // Alternatively, you could throw an error or require more specific configuration.
                    Loggers.ActiveDirectoryLogger.Information($"ADSettings: UseTLS is true, port is {settings.ServerPort} (non-standard for TLS inference). Attempting LDAPS as a fallback secure method.");
                    ConnectWithLdaps(settings, out connection, serverHostname);
                }
                if (connection == null)
                    return null;
                var appConnection = new AppLdapConnection(connection, startedTLS);

                if (_disposerTimer == null)
                {
                    _disposerTimer = new Timer(CleanPool, null, 30000, 30000);
                }


                _connectionPool.Add(appConnection);

                OnCountChanged?.Invoke();
                return appConnection;

            }

        }

        private void CleanPool(object? state)
        {
            lock (_poolLock)
            {
                try
                {
                    var count = _connectionPool.Count;
                    if (count < PoolSize) return;
                    for (int i = 0; i < count - PoolSize; i++)
                    {
                        if (!_connectionPool[i].IsDisposed
                            && _connectionPool[i].Expires != null
                            && _connectionPool[i].Expires < DateTime.Now
                            && _connectionPool.Count(x => x.Expires != null && !x.IsDisposed) > PoolSize)
                        {

                            _connectionPool[i].DisposeNow();

                            _connectionPool.RemoveAt(i);

                            i--;
                            count--;
                            OnCountChanged?.Invoke();
                        }

                    }
                    if (_connectionPool.Count < PoolSize && _connectionSettingsCache != null)
                    {
                        for (int i = 0; i < PoolSize - _connectionPool.Count; i++)
                        {
                            var conn = Connect(_connectionSettingsCache);
                            if (conn != null)
                            {
                                conn.Dispose();
                                _connectionPool.Add(conn);

                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectoryLogger.Information(ex, "Error during connection pool cleanup.");
                }

            }
        }

        /// <summary>
        /// Clears the pool of connections closing all active sessions
        /// </summary>
        public void ClearPool()
        {
            lock (_poolLock)
            {
                try
                {
                    var count = _connectionPool.Count;
                    for (int i = 0; i < count; i++)
                    {

                        _connectionPool[i].DisposeNow();


                    }
                    _connectionPool.Clear();


                    OnCountChanged?.Invoke();

                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectoryLogger.Information(ex, "Error during connection pool clearing.");
                }

            }
        }


        /// <summary>
        /// Establishes a secure LDAP connection using LDAPS (LDAP over SSL/TLS).
        /// </summary>
        /// <param name="ldapServerHost">The hostname or IP address of the LDAP server.</param>
        /// <param name="ldapServerPort">The LDAPS port (typically 636).</param>
        /// <param name="username">The username (e.g., "user@domain.com" or "DOMAIN\user") for binding.</param>
        /// <param name="password">The password for the user.</param>
        /// <param name="connection">The established LdapConnection object if successful, otherwise null.</param>
        /// <returns>True if the connection was successful, otherwise false.</returns>
        public static bool ConnectWithLdaps(ADSettings settings, out LdapConnection? connection, string? serverHostname=null)
        {
            connection = null;
            if (string.IsNullOrEmpty(settings.ServerAddress) || settings.ServerPort <= 0 || string.IsNullOrEmpty(settings.Username) || settings.Password == null)
            {
                Loggers.ActiveDirectoryLogger.Information("ConnectWithLdaps: Invalid parameters (host, port, username, or password).");
                return false;
            }

            try
            {
                //TestConnectionMethods(settings);

                // 1. Create LdapConnection object targeting the LDAPS port
                LdapDirectoryIdentifier identifier = new LdapDirectoryIdentifier(serverHostname??settings.ServerAddress, settings.ServerPort);
                connection = new LdapConnection(identifier);

                // 2. Specify that SSL should be used
                connection.AuthType = AuthType.Basic;
                connection.SessionOptions.SecureSocketLayer = true;
                connection.SessionOptions.ProtocolVersion = 3;
                connection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;



                // 4. Provide credentials
                NetworkCredential credential = new NetworkCredential(settings.Username + "@" + settings.FQDN, settings.Password.Decrypt().ToSecureString());
                connection.Credential = credential;
                // 5. Bind to the server (establish the connection and authenticate)
                Loggers.ActiveDirectoryLogger.Information($"Attempting LDAPS connection to {serverHostname ?? settings.ServerAddress}:{settings.ServerPort} as {settings.Username}...");
                connection.Bind();

                Loggers.ActiveDirectoryLogger.Information("LDAPS connection successful!");
                return true;
            }
            catch (LdapException ldapEx)
            {
                Loggers.ActiveDirectoryLogger.Debug($"LDAP Exception during LDAPS connection: {ldapEx.Message} (ErrorCode: {ldapEx.ErrorCode})");
                if (ldapEx.ServerErrorMessage != null)
                {
                    Loggers.ActiveDirectoryLogger.Debug($"Server Error Message: {ldapEx.ServerErrorMessage}");
                }
                if (connection != null)
                {
                    connection.Dispose();
                    connection = null;
                }
                return false;
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Error(ex, "General Exception during LDAPS connection");
                if (connection != null)
                {
                    connection.Dispose();
                    connection = null;
                }
                return false;
            }
        }



        /// <summary>
        /// Establishes a secure LDAP connection using StartTLS.
        /// </summary>
        /// <param name="ldapServerHost">The hostname or IP address of the LDAP server.</param>
        /// <param name="ldapServerPort">The standard LDAP port (typically 389).</param>
        /// <param name="username">The username (e.g., "user@domain.com" or "DOMAIN\user") for binding.</param>
        /// <param name="password">The password for the user.</param>
        /// <param name="connection">The established LdapConnection object if successful, otherwise null.</param>
        /// <returns>True if the connection was successful, otherwise false.</returns>
        public static bool ConnectWithStartTls(ADSettings settings, out LdapConnection? connection, string? serverHostname = null)
        {


            connection = null;
            if (string.IsNullOrEmpty(settings.ServerAddress) || settings.ServerPort <= 0 || string.IsNullOrEmpty(settings.Username) || settings.Password == null)
            {
                Loggers.ActiveDirectoryLogger.Information("ConnectWithStartTls: Invalid parameters (host, port, username, or password).");
                return false;
            }

            try
            {

                // 1. Create LdapConnection object targeting the standard LDAP port
                LdapDirectoryIdentifier identifier = new LdapDirectoryIdentifier(serverHostname ?? settings.ServerAddress, settings.ServerPort);
                var currentConnection = new LdapConnection(identifier);

                // 3. Provide credentials
                NetworkCredential credential = new NetworkCredential(settings.Username + "@" + settings.FQDN, settings.Password.Decrypt().ToSecureString());
                currentConnection.SessionOptions.ProtocolVersion = 3;
                currentConnection.Timeout = TimeSpan.FromSeconds(5);
                if (OperatingSystem.IsWindows())
                {
                    currentConnection.SessionOptions.TcpKeepAlive = true;
                    currentConnection.SessionOptions.Signing = true;
                    currentConnection.SessionOptions.Sealing = true;
                }
                else
                {
                    currentConnection.AuthType = AuthType.Basic;

                }
                currentConnection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;


                if (OperatingSystem.IsLinux() && settings.UseTLS && !StartTls(currentConnection))
                {
                    Loggers.ActiveDirectoryLogger.Information("StartTLS failed to initiate.");
                    return false;
                }


                currentConnection.Credential = credential;

                // 5. Bind to the server
                Loggers.ActiveDirectoryLogger.Information($"Attempting initial connection to {serverHostname ?? settings.ServerAddress}:{settings.ServerPort} for StartTLS as {settings.Username}...");
                currentConnection.Bind();
                currentConnection.AutoBind = true;
                connection = currentConnection;
                return true;
            }
            catch (LdapException ldapEx)
            {
                if (ldapEx.ErrorCode == 82)
                {
                    throw new CriticalActiveDirectoryException(null, "Certificate provided by Active Directory is not trusted by the machine running Blazam.");
                }
                if (ldapEx.ErrorCode == 49)
                {
                    throw new CriticalActiveDirectoryException(null, "The supplied credential is invalid.");
                }
                Loggers.ActiveDirectoryLogger.Information(ldapEx, "LDAP Exception during connection");
                if (ldapEx.ServerErrorMessage != null)
                {
                    Loggers.ActiveDirectoryLogger.Information(ldapEx, "Server Error");
                }
                if (connection != null)
                {
                    connection.Dispose();
                    connection = null;
                }
                return false;
            }
            
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Warning(ex, "General Exception during StartTLS connection");
                if (connection != null)
                {
                    connection.Dispose();
                    connection = null;
                }
                return false;
            }
        }

        private static bool StartTls(LdapConnection currentConnection)
        {
            if (OperatingSystem.IsWindows()) throw new AppException("Windows should use the signing and sealing options, not directly call StartTLS");
            lock (_tlsLock)
            {
              currentConnection.SessionOptions.StartTransportLayerSecurity(null);
                return true;
            }
        }
        public static bool StopTls(LdapConnection currentConnection)
        {
            if (OperatingSystem.IsLinux())
            {
                lock (_tlsLock)
                {
                    var tlsStarted = false;
                    int attempts = 0;

                    while (!tlsStarted && attempts < 5)
                    {
                        attempts++;
                        // Create a Task for the blocking operation
                        // Use a CancellationTokenSource to allow for cancellation attempts if the task gets "stuck"
                        using (var cancellationTokenSource = new CancellationTokenSource())
                        {
                            var connectionTask = Task.Run(() =>
                            {
                                try
                                {

                                    currentConnection.SessionOptions.StopTransportLayerSecurity();

                                    tlsStarted = true;
                                }
                                catch (OperationCanceledException)
                                {
                                    Loggers.ActiveDirectoryLogger.Warning($"StopTransportLayerSecurity was cancelled for {currentConnection.SessionOptions.HostName}.");
                                    throw; // Re-throw to be caught outside
                                }
                            }, cancellationTokenSource.Token);
                            var timeout = Stopwatch.StartNew();
                            while (currentConnection.SessionOptions.SecureSocketLayer == false && timeout.Elapsed < TimeSpan.FromSeconds(10))
                            {
                                Task.Delay(50).Wait();
                            }

                            if (!currentConnection.SessionOptions.SecureSocketLayer)
                            {
                                return false;
                            }
                            timeout.Stop();

                        }
                    }

                }
            }
            return true;

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                ClearPool();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
