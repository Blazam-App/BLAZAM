using System;
using System.Collections.Concurrent;
using System.DirectoryServices.Protocols;
using System.Net; // Required for NetworkCredential
using BLAZAM.ActiveDirectory.Data;
using BLAZAM.Database.Models;
using BLAZAM.Helpers;
using BLAZAM.Logger; // Added for ADSettings

namespace BLAZAM.ActiveDirectory
{
    public static class SecureLdapConnector
    {
        private static Timer? _disposerTimer = null;
        private static object _lock = new object();
        private static List<AppLdapConnection> _connectionPool = new();
        private static bool _testsPerformed;

        /// <summary>
        /// Establishes a secure LDAP connection based on ADSettings.
        /// It will choose LDAPS if port is typically 636 and UseTLS is true,
        /// or StartTLS if port is typically 389 and UseTLS is true.
        /// </summary>
        /// <param name="settings">The ADSettings object containing connection parameters.</param>
        /// <param name="connection">The established LdapConnection object if successful, otherwise null.</param>
        /// <returns>True if the connection was successful, otherwise false.</returns>
        public static AppLdapConnection? Connect(ADSettings settings)
        {
            //lock (_lock)
            //{
            //    foreach (var conn in _connectionPool)
            //    {
            //        if (conn.Expires != null)
            //        {
            //            conn.Expires = null;
            //            return conn;
            //        }
            //    }

            //}
            LdapConnection connection = null;
            if (settings == null)
            {
                Loggers.ActiveDirectoryLogger.Information("ADSettings object is null.");
                return default;
            }

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



            // Typically, port 636 is for LDAPS, and 389 is for LDAP (which can be upgraded with StartTLS).
            // We'll infer the method based on common port usage when UseTLS is true.
            if (settings.ServerPort == 636) // Common LDAPS port
            {
                Loggers.ActiveDirectoryLogger.Information($"ADSettings: UseTLS is true, port is {settings.ServerPort}. Attempting LDAPS connection.");
                ConnectWithLdaps(settings.ServerAddress, settings.ServerPort, settings.Username, settings.Password.Decrypt(), out connection);
            }
            else if (settings.ServerPort == 389) // Common LDAP port, suitable for StartTLS
            {
                Loggers.ActiveDirectoryLogger.Information($"ADSettings: UseTLS is true, port is {settings.ServerPort}. Attempting StartTLS connection.");
                ConnectWithStartTls(settings.ServerAddress, settings.ServerPort, settings.Username, settings.Password.Decrypt(), out connection);
            }
            else
            {
                // If UseTLS is true but port is neither 389 nor 636, it's ambiguous.
                // For this example, we'll try LDAPS as a default secure method if UseTLS is true and port is non-standard.
                // Alternatively, you could throw an error or require more specific configuration.
                Loggers.ActiveDirectoryLogger.Information($"ADSettings: UseTLS is true, port is {settings.ServerPort} (non-standard for TLS inference). Attempting LDAPS as a fallback secure method.");
                ConnectWithLdaps(settings.ServerAddress, settings.ServerPort, settings.Username, settings.Password, out connection);
            }
            var appConnection = new AppLdapConnection(connection);
            lock (_lock)
            {
                if (_disposerTimer == null)
                {
                    _disposerTimer = new Timer(CleanPool, null, 30000, 30000);
                }
                _connectionPool.Add(appConnection);
            }
            return appConnection;
        }

        private static void CleanPool(object? state)
        {
            lock (_lock)
            {
                try
                {
                    var count = _connectionPool.Count;
                    for (int i = 0; i < count; i++)
                    {
                        if (!_connectionPool[i].IsDisposed && _connectionPool[i].Expires != null && _connectionPool[i].Expires < DateTime.Now)
                        {
                            _connectionPool[i].DisposeNow();
                            _connectionPool.RemoveAt(i);
                            i--;
                            count--;
                        }

                    }
                }
                catch (Exception ex)
                {

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
        public static bool ConnectWithLdaps(string ldapServerHost, int ldapServerPort, string username, string password, out LdapConnection? connection)
        {
            connection = null;
            if (string.IsNullOrEmpty(ldapServerHost) || ldapServerPort <= 0 || string.IsNullOrEmpty(username) || password == null)
            {
                Loggers.ActiveDirectoryLogger.Information("ConnectWithLdaps: Invalid parameters (host, port, username, or password).");
                return false;
            }

            try
            {
                TestConnectionMethods(ldapServerHost, ldapServerPort, username, password);

                // 1. Create LdapConnection object targeting the LDAPS port
                LdapDirectoryIdentifier identifier = new LdapDirectoryIdentifier(ldapServerHost, ldapServerPort);
                connection = new LdapConnection(identifier);

                // 2. Specify that SSL should be used
                connection.AuthType = AuthType.Basic;
                connection.SessionOptions.SecureSocketLayer = true;
                connection.SessionOptions.ProtocolVersion = 3;

               
                // 4. Provide credentials
                NetworkCredential credential = new NetworkCredential(username, password);
                connection.Credential = credential;

                // 5. Bind to the server (establish the connection and authenticate)
                Loggers.ActiveDirectoryLogger.Information($"Attempting LDAPS connection to {ldapServerHost}:{ldapServerPort} as {username}...");
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
                Loggers.ActiveDirectoryLogger.Error($"General Exception during LDAPS connection: {ex.Message}");
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
        public static bool ConnectWithStartTls(string ldapServerHost, int ldapServerPort, string username, string password, out LdapConnection? connection)
        {


            connection = null;
            if (string.IsNullOrEmpty(ldapServerHost) || ldapServerPort <= 0 || string.IsNullOrEmpty(username) || password == null)
            {
                Loggers.ActiveDirectoryLogger.Information("ConnectWithStartTls: Invalid parameters (host, port, username, or password).");
                return false;
            }

            try
            {
                TestConnectionMethods(ldapServerHost, ldapServerPort, username, password);
                // 1. Create LdapConnection object targeting the standard LDAP port
                LdapDirectoryIdentifier identifier = new LdapDirectoryIdentifier(ldapServerHost, ldapServerPort);
                connection = new LdapConnection(identifier);

                // 2. (Optional but Recommended) Configure server certificate validation
                // connection.SessionOptions.VerifyServerCertificate = new VerifyServerCertificateCallback(ServerCallback);

                // 3. Provide credentials
                NetworkCredential credential = new NetworkCredential(username, password);
                connection.Credential = credential;
                connection.SessionOptions.ProtocolVersion = 3;
                connection.AuthType = AuthType.Negotiate;
                connection.SessionOptions.Signing = true;
                connection.SessionOptions.Sealing = true;
                //connection.SessionOptions.VerifyServerCertificate = (state,crt) => { return true; };


                
                // 5. Bind to the server
                Loggers.ActiveDirectoryLogger.Information($"Attempting initial connection to {ldapServerHost}:{ldapServerPort} for StartTLS as {username}...");
                connection.Bind();


                //Loggers.ActiveDirectoryLogger.Information("StartTLS successful! Connection is now secure.");
                return true;
            }
            catch (LdapException ldapEx)
            {
                Loggers.ActiveDirectoryLogger.Information(ldapEx, "LDAP Exception during StartTLS connection: {@ldapEx}");
                if (ldapEx.ServerErrorMessage != null)
                {
                    Loggers.ActiveDirectoryLogger.Information($"Server Error Message: {ldapEx.ServerErrorMessage}");
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
                Loggers.ActiveDirectoryLogger.Warning($"General Exception during StartTLS connection: {ex.Message}");
                if (connection != null)
                {
                    connection.Dispose();
                    connection = null;
                }
                return false;
            }
        }

        private static void TestConnectionMethods(string ldapServerHost, int ldapServerPort, string username, string password)
        {
            if (!_testsPerformed)
            {
                _testsPerformed = true;

                // Define connection scenarios to test
                var scenarios = new[] { "Plain", "StartTLS", "LDAPS" };
                // Define authentication types to test
                var authTypes = new[] { AuthType.Basic };

                bool[]? boolOptions = new[] { false, true };
                // Loop through every combination
                foreach (var authType in authTypes)
                {
                    foreach (var sslOption in boolOptions)
                    {
                        foreach (var signingOption in boolOptions)
                        {
                            foreach (var tlsOption in boolOptions)
                            {
                                foreach (var certOption in boolOptions)
                                {
                                    foreach (var sealingOption in boolOptions)
                                    {
                                        var optionsString = $"Signing ={signingOption}, TLS ={tlsOption}, SSL ={sslOption}, Sealing ={sealingOption}, IgnoreCert ={certOption}, Auth ={authType}";
                                        //Use a new LdapConnection for each attempt
                                        LdapConnection connection2 = null;
                                        // Set port based on scenario. LDAPS typically uses 636.

                                        Loggers.ActiveDirectoryLogger.Information($"========== TESTING {optionsString} ==========");


                                        try
                                        {
                                            // 1. Create LdapConnection object for this specific test
                                            LdapDirectoryIdentifier identifier2 = new LdapDirectoryIdentifier(ldapServerHost, ldapServerPort);
                                            connection2 = new LdapConnection(identifier2);

                                            // 2. Provide credentials
                                            
                                            connection2.Credential = new NetworkCredential(username, password);
                                            connection2.SessionOptions.ProtocolVersion = 3;
                                            connection2.AuthType = authType;
                                            connection2.SessionOptions.ReferralChasing = ReferralChasingOptions.None;

                                            connection2.SessionOptions.SecureSocketLayer = sslOption;
                                            if(signingOption)
                                                connection2.SessionOptions.Signing = signingOption;
                                            if(sealingOption)
                                                connection2.SessionOptions.Sealing = sealingOption;

                                            if (certOption)
                                            {
                                                connection2.SessionOptions.VerifyServerCertificate = (conn, cert) =>
                                                {
                                                    Loggers.ActiveDirectoryLogger.Information($"Server certificate presented for {optionsString}. Subject: {cert.Subject}. Accepting for test purposes.");
                                                    return true;
                                                };
                                            }
                                            if (tlsOption)
                                            {
                                                connection2.SessionOptions.StartTransportLayerSecurity(null);
                                            }
                                            // 4. (For Diagnostics) Bypass server certificate validation to isolate other errors.
                                            // WARNING: This is insecure and should ONLY be used for testing.


                                            // 5. Bind to the server
                                            Loggers.ActiveDirectoryLogger.Information($"Attempting Bind() to {ldapServerHost}:{ldapServerPort} as {username}...");
                                            connection2.Bind();

                                            Loggers.ActiveDirectoryLogger.Information($"========== SUCCESS:{optionsString} ==========");

                                        }
                                        catch (LdapException ldapEx)
                                        {
                                            // Log the full LdapException, which includes error codes and server messages.
                                            Loggers.ActiveDirectoryLogger.Information(ldapEx, $"LDAP Exception on {optionsString}: {@ldapEx}");
                                            if (ldapEx.ServerErrorMessage != null)
                                            {
                                                Loggers.ActiveDirectoryLogger.Information($"Server Error Message: {ldapEx.ServerErrorMessage}");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            // Log the full General Exception. ex.ToString() is critical as it includes the InnerException.
                                            Loggers.ActiveDirectoryLogger.Warning($"General Exception on {optionsString}: {ex.ToString()}");
                                        }
                                        finally
                                        {
                                            // Always dispose of the connection object to release resources
                                            if (connection2 != null)
                                            {
                                                connection2.Dispose();
                                            }
                                            Loggers.ActiveDirectoryLogger.Information($"========== FINISHED TEST: {optionsString} ==========\n");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Optional: Custom server certificate validation callback.
        /// Use with caution, especially in production.
        /// For production, ensure your LDAP server has a valid certificate from a trusted CA.
        /// </summary>
        private static bool ServerCallback(LdapConnection connection, System.Security.Cryptography.X509Certificates.X509Certificate certificate)
        {
            Console.WriteLine($"Server certificate issued to: {certificate.Subject}");
            Console.WriteLine($"Server certificate issued by: {certificate.Issuer}");

            System.Security.Cryptography.X509Certificates.X509Chain chain = new System.Security.Cryptography.X509Certificates.X509Chain();
            System.Security.Cryptography.X509Certificates.X509ChainPolicy chainPolicy = new System.Security.Cryptography.X509Certificates.X509ChainPolicy
            {
                RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck
            };
            bool isValid = chain.Build(new System.Security.Cryptography.X509Certificates.X509Certificate2(certificate));
            if (isValid)
            {
                Console.WriteLine("Server certificate is valid according to system validation within callback.");
                return true;
            }
            else
            {
                Console.WriteLine("Server certificate is INVALID according to system validation within callback.");
                foreach (var status in chain.ChainStatus)
                {
                    Console.WriteLine($"  - {status.Status}: {status.StatusInformation}");
                }
                return false;
            }
        }


    }
}
