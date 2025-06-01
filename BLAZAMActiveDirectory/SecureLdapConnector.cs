using System;
using System.DirectoryServices.Protocols;
using System.Net; // Required for NetworkCredential
using BLAZAM.Database.Models;
using BLAZAM.Helpers; // Added for ADSettings

namespace BLAZAM.ActiveDirectory
{
    public static class SecureLdapConnector
    {
        /// <summary>
        /// Establishes a secure LDAP connection based on ADSettings.
        /// It will choose LDAPS if port is typically 636 and UseTLS is true,
        /// or StartTLS if port is typically 389 and UseTLS is true.
        /// </summary>
        /// <param name="settings">The ADSettings object containing connection parameters.</param>
        /// <param name="connection">The established LdapConnection object if successful, otherwise null.</param>
        /// <returns>True if the connection was successful, otherwise false.</returns>
        public static LdapConnection? Connect(ADSettings settings)
        {
            LdapConnection connection = null;
            if (settings == null)
            {
                Console.WriteLine("ADSettings object is null.");
                return connection;
            }

            // Optional: Check the IsValid property from ADSettings, though the individual Connect methods will also fail if parameters are bad.
            // if (!settings.IsValid)
            // {
            //     Console.WriteLine("ADSettings are invalid according to its IsValid property.");
            //     return false;
            // }

            if (string.IsNullOrEmpty(settings.ServerAddress))
            {
                Console.WriteLine("ServerAddress in ADSettings is null or empty.");
                return connection;
            }
            if (string.IsNullOrEmpty(settings.Username))
            {
                Console.WriteLine("Username in ADSettings is null or empty.");
                return connection;
            }
            if (string.IsNullOrEmpty(settings.Password))
            {
                Console.WriteLine("Password in ADSettings is null or empty.");
                return connection;
            }



            // Typically, port 636 is for LDAPS, and 389 is for LDAP (which can be upgraded with StartTLS).
            // We'll infer the method based on common port usage when UseTLS is true.
            if (settings.ServerPort == 636) // Common LDAPS port
            {
                Console.WriteLine($"ADSettings: UseTLS is true, port is {settings.ServerPort}. Attempting LDAPS connection.");
                ConnectWithLdaps(settings.ServerAddress, settings.ServerPort, settings.Username, settings.Password, out connection);
            }
            else if (settings.ServerPort == 389) // Common LDAP port, suitable for StartTLS
            {
                Console.WriteLine($"ADSettings: UseTLS is true, port is {settings.ServerPort}. Attempting StartTLS connection.");
                ConnectWithStartTls(settings.ServerAddress, settings.ServerPort, settings.Username, settings.Password.Decrypt(), out connection);
            }
            else
            {
                // If UseTLS is true but port is neither 389 nor 636, it's ambiguous.
                // For this example, we'll try LDAPS as a default secure method if UseTLS is true and port is non-standard.
                // Alternatively, you could throw an error or require more specific configuration.
                Console.WriteLine($"ADSettings: UseTLS is true, port is {settings.ServerPort} (non-standard for TLS inference). Attempting LDAPS as a fallback secure method.");
                ConnectWithLdaps(settings.ServerAddress, settings.ServerPort, settings.Username, settings.Password, out connection);
            }
            return connection;
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
                Console.WriteLine("ConnectWithLdaps: Invalid parameters (host, port, username, or password).");
                return false;
            }

            try
            {
                // 1. Create LdapConnection object targeting the LDAPS port
                LdapDirectoryIdentifier identifier = new LdapDirectoryIdentifier(ldapServerHost, ldapServerPort);
                connection = new LdapConnection(identifier);

                // 2. Specify that SSL should be used
                connection.SessionOptions.SecureSocketLayer = true;

                // 3. (Optional but Recommended) Configure server certificate validation
                // connection.SessionOptions.VerifyServerCertificate = new VerifyServerCertificateCallback(ServerCallback);

                // 4. Provide credentials
                NetworkCredential credential = new NetworkCredential(username, password);
                connection.Credential = credential;

                // 5. Bind to the server (establish the connection and authenticate)
                Console.WriteLine($"Attempting LDAPS connection to {ldapServerHost}:{ldapServerPort} as {username}...");
                connection.Bind();

                Console.WriteLine("LDAPS connection successful!");
                return true;
            }
            catch (LdapException ldapEx)
            {
                Console.WriteLine($"LDAP Exception during LDAPS connection: {ldapEx.Message} (ErrorCode: {ldapEx.ErrorCode})");
                if (ldapEx.ServerErrorMessage != null)
                {
                    Console.WriteLine($"Server Error Message: {ldapEx.ServerErrorMessage}");
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
                Console.WriteLine($"General Exception during LDAPS connection: {ex.Message}");
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
                Console.WriteLine("ConnectWithStartTls: Invalid parameters (host, port, username, or password).");
                return false;
            }

            try
            {
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



                // 5. Bind to the server
                Console.WriteLine($"Attempting initial connection to {ldapServerHost}:{ldapServerPort} for StartTLS as {username}...");
                connection.Bind();


                Console.WriteLine("StartTLS successful! Connection is now secure.");
                return true;
            }
            catch (LdapException ldapEx)
            {
                Console.WriteLine($"LDAP Exception during StartTLS connection: {ldapEx.Message} (ErrorCode: {ldapEx.ErrorCode})");
                if (ldapEx.ServerErrorMessage != null)
                {
                    Console.WriteLine($"Server Error Message: {ldapEx.ServerErrorMessage}");
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
                Console.WriteLine($"General Exception during StartTLS connection: {ex.Message}");
                if (connection != null)
                {
                    connection.Dispose();
                    connection = null;
                }
                return false;
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
