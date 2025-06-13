
using AngleSharp.Dom;
using Azure;
using BLAZAM.ActiveDirectory.Data;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Services;
using BLAZAM.Common.Exceptions;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Novell.Directory.Ldap;
using System.Collections;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Text;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class LdapDirectoryEntry : IDirectoryEntry
    {
        public readonly DirectoryEntry UnderlyingEntry;
        private static Dictionary<string, AttributeSchemaInfo> _schemaCache { get; } = new();
        protected IActiveDirectoryContext Directory { get; set; }
        private bool disposedValue;
        private static readonly object _namingContextLock = new();
        private static readonly object _schemaLock = new();
        private static string _namingContextCache;

        public LdapDirectoryEntry(string dn, IActiveDirectoryContext directory)
        {
            DN = dn;
            Directory = directory;
        }
        public LdapDirectoryEntry(SearchResultEntry sre, IActiveDirectoryContext directory)
        {
            DN = sre.DistinguishedName;
            Directory = directory;
            Dictionary<string, object> entryAttributes = new();


        }

        public string Path { get => UnderlyingEntry.Path; set => UnderlyingEntry.Path = value; }

        public string? NativeGuid => GetPropertyValue("nativeGuid")?.ToString();

        public void SetPropertyValue(string propertyName, object? value)
        {
            Invoke(propertyName, DirectoryAttributeOperation.Replace, value);
            DirectoryCache.Clear(DN);
            return;
        }
        public void RemovePropertyValue(string propertyName, object? value)
        {
            Invoke(propertyName, DirectoryAttributeOperation.Delete, value);
            return;
        }
        public void AddPropertyValue(string propertyName, object? value)
        {
            Invoke(propertyName, DirectoryAttributeOperation.Add, value);
            return;
        }
        public void ClearPropertyValue(string propertyName)
        {
            Invoke(propertyName, DirectoryAttributeOperation.Replace, null);
            return;
        }
        public bool ContainsProperty(string propertyName)
        {
            var existingCache = DirectoryCache.GetEntryCache(DN);
            if (existingCache != null && existingCache.Attributes.ContainsKey(propertyName.ToLower()))
            {
                return existingCache.Attributes.ContainsKey(propertyName.ToLower());
            }
            else
            {
                Search(propertyName);
                existingCache = DirectoryCache.GetEntryCache(DN);
                return existingCache?.Attributes.ContainsKey(propertyName.ToLower())==true;

            }
        }
        public bool PropertyContains(string propertyName, object value)
        {
            var val = GetPropertyValue(propertyName);
            if (val is object[] array)
            {
                return array.Contains(value);
            }
            else
            {
                return value.Equals(val);
            }
        }
        public object? GetPropertyValue(string propertyName)
        {
            return Search(propertyName);
        }

        public string? DN { get; set; }


        private bool _propertiesCollected = false;

   
        private object? Search(string attributeName)
        {

            var existingCache = DirectoryCache.GetEntryCache(DN);
            if (existingCache != null)
            {
                if (existingCache.Attributes.ContainsKey(attributeName.ToLower()))
                {
                    return existingCache.Attributes[attributeName.ToLower()];
                }
                else if (_propertiesCollected)
                {
                    if (!existingCache.Attributes.ContainsKey(attributeName.ToLower()))
                    {
                        existingCache.Attributes[attributeName.ToLower()] = null;
                    }
                    return existingCache.Attributes[attributeName.ToLower()];
                }
            }
            else
            {
                existingCache = new(new());
            }

            Loggers.ActiveDirectoryLogger.Information("Creating ldapConnection in LdapDirectoryEntry {@DirectoryNotNull}", Directory != null && Directory.ConnectionSettings != null);
            using var ldapConnection = SecureLdapConnector.Connect(Directory.ConnectionSettings);
            GetNamingContext(ldapConnection);




            // Search request to get ALL user attributes for the specified DN
            SearchRequest allAttributesSearchRequest = new SearchRequest(
                DN,                                 // The DN of the object
                "(objectClass=*)",                  // Filter to match the object
                System.DirectoryServices.Protocols.SearchScope.Base, // Target a specific object
                null                                // Request all user attributes
            );



            SearchResponse searchResponse = (SearchResponse)ldapConnection.LdapConnection.SendRequest(allAttributesSearchRequest);



            SearchResultEntry entry = searchResponse.Entries[0];
            foreach (string currentAttributeLdapName in entry.Attributes.AttributeNames)
            {


                DirectoryAttribute directoryAttribute = entry.Attributes[currentAttributeLdapName];
                GetSchemaInfo(ldapConnection, currentAttributeLdapName);

                if (_schemaCache.ContainsKey(currentAttributeLdapName) && _schemaCache[currentAttributeLdapName] == null)
                {
                    // Schema not found for this attribute.
                    // Cache null for this attribute, consistent with original behavior for unresolvable schema.
                    existingCache.Attributes[currentAttributeLdapName.ToLower()] = null;
                    // Log: $"Schema not found for attribute '{currentAttributeLdapName}'. Caching as null."
                    Console.WriteLine($"Warning: Schema not found for attribute '{currentAttributeLdapName}'. It will be cached as null.");
                    continue;
                }

                try
                {
                    if (directoryAttribute.Count == 0)
                    {
                        existingCache.Attributes[currentAttributeLdapName.ToLower()] = null;
                    }
                    else if (_schemaCache[currentAttributeLdapName].IsSingleValued)
                    {
                        // Assuming ConvertSingleValue is accessible
                        existingCache.Attributes[currentAttributeLdapName.ToLower()] = ConvertSingleValue(directoryAttribute[0], currentAttributeLdapName);
                    }
                    else
                    {
                        List<object> values = new List<object>();
                        foreach (object rawValue in directoryAttribute)
                        {
                            values.Add(ConvertSingleValue(rawValue,currentAttributeLdapName));
                        }
                        existingCache.Attributes[currentAttributeLdapName.ToLower()] = values.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    // Log: $"Error converting attribute '{currentAttributeLdapName}': {ex.Message}. Caching as null."
                    Console.WriteLine($"Error converting attribute '{currentAttributeLdapName}': {ex.Message}. It will be cached as null.");
                    existingCache.Attributes[currentAttributeLdapName.ToLower()] = null; // Cache null if conversion fails
                }
            }
            if (!existingCache.Attributes.ContainsKey(attributeName.ToLower()))
            {
                existingCache.Attributes[attributeName.ToLower()] = null;
            }
            DirectoryCache.SetEntryCache(DN, existingCache.Attributes);
            _propertiesCollected = true;
            return existingCache.Attributes[attributeName.ToLower()]; // Attribute not found on the object or no value



        }

        private static void GetNamingContext(AppLdapConnection? ldapConnection)
        {
            if (_namingContextCache.IsNullOrEmpty())
            {

                lock (_namingContextLock)
                {
                    if (_namingContextCache.IsNullOrEmpty())
                    {
                        // First, find the schema naming context
                        var rootDseRequest = new SearchRequest("", "(objectClass=*)", System.DirectoryServices.Protocols.SearchScope.Base, "schemaNamingContext");
                        var rootDseResponse = (SearchResponse)ldapConnection.LdapConnection.SendRequest(rootDseRequest);
                        if (rootDseResponse.Entries.Count == 0)
                        {
                            throw new AppException("Could not read RootDSE to find schema naming context.");
                        }
                        string schemaNamingContext = rootDseResponse.Entries[0].Attributes["schemaNamingContext"][0].ToString();

                        _namingContextCache = schemaNamingContext;
                    }
                }
            }
        }

        private void GetSchemaInfo(AppLdapConnection ldapConnection, string propertyName)
        {
            if (!_schemaCache.ContainsKey(propertyName))
            {

                SearchRequest schemaSearchRequest = new SearchRequest(
                    _namingContextCache,
                    $"(&(objectClass=attributeSchema)(ldapDisplayName={propertyName}))",
                    System.DirectoryServices.Protocols.SearchScope.Subtree,
                    "attributeSyntax", "oMSyntax", "isSingleValued", "oMObjectClass", "cn" // "cn" can be useful for debugging
                );

                try
                {
                    lock (_schemaCache)
                    {
                        if (!_schemaCache.ContainsKey(propertyName))
                        {


                            SearchResponse schemaSearchResponse = (SearchResponse)ldapConnection.LdapConnection.SendRequest(schemaSearchRequest);
                            if (schemaSearchResponse.Entries.Count > 0)
                            {
                                SearchResultEntry schemaEntry = schemaSearchResponse.Entries[0];
                                var info = new AttributeSchemaInfo
                                {
                                    AtttributeName = propertyName, // Use the name we looked up by
                                    AttributeSyntax = schemaEntry.Attributes["attributeSyntax"][0].ToString(),
                                    OMSyntax = int.Parse(schemaEntry.Attributes["oMSyntax"][0].ToString()),
                                    IsSingleValued = bool.Parse(schemaEntry.Attributes["isSingleValued"][0].ToString()),
                                    OMObjectClass = schemaEntry.Attributes.Contains("omObjectClass") && schemaEntry.Attributes["omObjectClass"][0] is byte[] omocBytes
                                                    ? Encoding.UTF8.GetString(omocBytes)
                                                    : (schemaEntry.Attributes.Contains("omObjectClass") ? schemaEntry.Attributes["omObjectClass"][0]?.ToString() : null)
                                };
                                _schemaCache.Add(propertyName, info);

                            }
                            else
                            {
                                // Log: $"Schema not found for attribute '{attributeLdapDisplayName}'."
                                Console.WriteLine($"Warning: Schema not found for attribute '{propertyName}'.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log: $"Error fetching schema for attribute '{attributeLdapDisplayName}': {ex.Message}"
                    Console.WriteLine($"Error fetching schema for attribute '{propertyName}': {ex.Message}. Schema will be considered not found.");
                }
            }
        }
        private static object ConvertSingleValue(object rawValue, string propertyName)
        {
            var schemaInfo = _schemaCache[propertyName];
            if (rawValue == null) return null;

            // Raw value from S.DS.P is often byte[] or string
            // We use oMSyntax for primary type determination

            switch (schemaInfo.OMSyntax)
            {
                case 1: // Boolean
                    if (rawValue is byte[] bytesBool) // Sometimes comes as byte array
                    {
                        if (bytesBool.Length > 0 && bytesBool[0] != 0) return true; // AD often uses 0xFF for TRUE
                        return false;
                    }
                    // AD often returns "TRUE" or "FALSE" as strings for booleans when using S.DS.DirectoryEntry
                    // With S.DS.Protocols, it's more often raw. But LDAP servers can differ.
                    // If it comes as string "TRUE" / "FALSE"
                    if (rawValue is string strBool)
                    {
                        if (bool.TryParse(strBool, out bool bResult)) return bResult; // Standard .NET bool parsing
                                                                                      // AD specific:
                        if (string.Equals(strBool, "TRUE", StringComparison.OrdinalIgnoreCase)) return true;
                        if (string.Equals(strBool, "FALSE", StringComparison.OrdinalIgnoreCase)) return false;
                    }
                    // If it comes as an integer-like value (though less common directly via S.DS.P for bool)
                    try { return Convert.ToInt32(rawValue) != 0; }
                    catch { /* fall through */ }
                    return rawValue; // Fallback

                case 2: // Integer
                case 10: // Integer / Enumeration
                    if (rawValue is byte[] bytesInt) return BitConverter.ToInt32(bytesInt, 0); // If raw BER encoded integer
                    return Convert.ToInt32(rawValue); // If string or other numeric

                case 4: // Octet String (e.g., SID, GUID)
                    if (rawValue is byte[] bytesOctet)
                    {
                        // Example: SID
                        if (string.Equals(schemaInfo.AtttributeName, "objectSid", StringComparison.OrdinalIgnoreCase))
                        {
                            return bytesOctet;
                        }
                        // Example: GUID
                        if (string.Equals(schemaInfo.AtttributeName, "objectGUID", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(schemaInfo.AtttributeName, "schemaIDGUID", StringComparison.OrdinalIgnoreCase))
                        {
                            return new Guid(bytesOctet).ToString();
                        }
                        return bytesOctet; // Return raw byte array if no specific handling
                    }
                    return rawValue; // Fallback

                case 6:  // String(Object-Identifier)
                case 18: // String(Numeric)
                case 19: // String(Printable)
                case 20: // String(Teletex)
                case 22: // String(IA5)
                case 27: // String(Case Sensitive)
                case 64: // String(Unicode)
                    if (rawValue is byte[] bytesStr) return Encoding.UTF8.GetString(bytesStr);
                    return rawValue.ToString();

                case 23: // UTC Time
                case 24: // Generalized Time
                    string timeStr = (rawValue is byte[] bytesTime) ? Encoding.ASCII.GetString(bytesTime) : rawValue.ToString();
                    // GeneralizedTime: yyyyMMddHHmmss.0Z  or yyyyMMddHHmmss.fZ (f can be multiple digits)
                    // UTCTime: yyMMddHHmmssZ or yyyyMMddHHmmssZ
                    // Need to handle potential variations and milliseconds carefully.
                    if (System.DateTime.TryParseExact(timeStr.TrimEnd('Z'),
                        new string[] { "yyyyMMddHHmmss.f", "yyyyMMddHHmmss", "yyMMddHHmmss" },
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                        out DateTime dt))
                    {
                        return dt;
                    }
                    // Try with explicit Z for older formats if ParseExact with TrimEnd('Z') fails on some inputs
                    if (System.DateTime.TryParseExact(timeStr,
                        new string[] { "yyyyMMddHHmmss.0Z", "yyyyMMddHHmmssZ", "yyMMddHHmmssZ" },
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                        out DateTime dtWithZ))
                    {
                        return dtWithZ;
                    }
                    return rawValue; // Fallback if parsing fails

                case 65: // Large Integer (Interval, accountExpires, pwdLastSet etc.)
                    string largeIntStr = (rawValue is byte[] bytesLI) ? Encoding.ASCII.GetString(bytesLI) : rawValue.ToString();
                    if (long.TryParse(largeIntStr, out long lResult))
                    {
                        // Special handling for FileTime-like attributes (e.g., accountExpires, lastLogon, pwdLastSet)
                        // These are often 64-bit integers representing 100-nanosecond intervals since Jan 1, 1601.
                        // 0 or max value (2^63 - 1) often means "never" or "not set".
                        if (lResult == 0 || lResult == 9223372036854775807L) // Max long
                        {
                            if (schemaInfo.AtttributeName.Equals("accountExpires", StringComparison.OrdinalIgnoreCase) ||
                                schemaInfo.AtttributeName.Equals("pwdLastSet", StringComparison.OrdinalIgnoreCase) ||
                                schemaInfo.AtttributeName.Contains("Logon")) // lastLogon, lastLogonTimestamp
                                return null; // Or a specific "Never" representation
                        }
                        try
                        {
                            return DateTime.FromFileTimeUtc(lResult);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            return lResult; // If not a valid FileTime, return the long
                        }
                    }
                    return rawValue; // Fallback

                case 66: // NT Security Descriptor
                    if (rawValue is byte[] bytesSD)
                    {
                        // Can convert to System.Security.AccessControl.RawSecurityDescriptor
                        // return new System.Security.AccessControl.RawSecurityDescriptor(bytesSD, 0);
                        // For simplicity, returning as string representation or bytes
                        return Convert.ToBase64String(bytesSD); // Or process further
                    }
                    return rawValue;

                case 127: // Object (often DN syntax, depends on oMObjectClass)
                          // oMObjectClass would ideally be decoded from BER to a dotted OID string
                          // and then mapped. For common cases like DNs:
                          // If oMObjectClass represents a DN syntax (e.g., "1.3.12.2.1011.28.0.714" for Object(DS-DN))
                    if (rawValue is byte[] bytesDN) return Encoding.UTF8.GetString(bytesDN);
                    return rawValue.ToString(); // Typically a Distinguished Name string

                default:
                    if (rawValue is byte[] bytesDefault)
                    {
                        // Attempt to decode as UTF8 string as a general fallback for unknown byte arrays
                        try { return Encoding.UTF8.GetString(bytesDefault); }
                        catch { return bytesDefault; /* Return raw bytes if not valid UTF8 */ }
                    }
                    return rawValue; // Return as is if no specific conversion
            }
        }

        public string SchemaClassName => UnderlyingEntry.SchemaClassName;

        public string Name => GetPropertyValue("name")?.ToString();

        public IDirectoryEntry Parent
        {
            get
            {
                return new LdapDirectoryEntry(DN.GetParentDn(), Directory);
            }
        }

        public IDirectoryEntries Children => new LdapDirectoryEntries(DN, Directory);

        public AuthType AuthenticationType { get => Directory.Connect().LdapConnection.AuthType; }
        public bool UsePropertyCache { get; set; }

        public void Close()
        {
            UnderlyingEntry.Close();
        }

        public void CommitChanges()
        {
            UnderlyingEntry.CommitChanges();
        }

        public object? Invoke(string methodName, params object[]? args)
        {
            switch (methodName)
            {
                case "SetPassword":
                    return SetUserPasswordLdap(args[0].ToString());

                case "Add":
                    return InvokeAdd(args);

                case "Remove":
                    return InvokeRemove(args);

            }
            return null;
            //return UnderlyingEntry.Invoke(methodName, args);
        }

        private bool InvokeRemove(object[]? args)
        {
            var value = args[0].ToString();
            return Invoke("member", DirectoryAttributeOperation.Delete, value);
        }

        private bool InvokeAdd(object[]? args)
        {
            var value = args[0].ToString();
            return Invoke("member", DirectoryAttributeOperation.Add, value);
        }




        /// <summary>
        /// Sets a user's password using System.DirectoryServices.Protocols.
        /// This is equivalent to DirectoryEntry.Invoke("SetPassword", new[] { newPasswordString }).
        /// </summary>
        /// <param name="ldapConnection">An active and bound LdapConnection. Must be secure (LDAPS or StartTLS).</param>
        /// <param name="userDistinguishedName">The Distinguished Name (DN) of the user account.</param>
        /// <param name="newPassword">The new password string.</param>
        /// <returns>True if the password was set successfully; otherwise, false.</returns>
        public bool SetUserPasswordLdap(string newPassword)
        {

            // The password for unicodePwd must be a UTF-16LE encoded string, enclosed in double quotes.

            string formattedPassword = "\"" + newPassword + "a\"";
            byte[] newPasswordBytes = Encoding.Unicode.GetBytes(formattedPassword);

            string formattedPassword2 = "\"" + newPassword + "\"";
            byte[] newPasswordBytes2 = Encoding.Unicode.GetBytes(formattedPassword2);

            if (Invoke("unicodePw", DirectoryAttributeOperation.Replace, newPasswordBytes))
            {
                return Invoke("unicodePw", DirectoryAttributeOperation.Replace, newPasswordBytes2);
            }
            return false;

        }

        private bool Invoke(string attributeName, DirectoryAttributeOperation operation, object? value = null)
        {

            using var ldapConnection = SecureLdapConnector.Connect(Directory.ConnectionSettings);

            //// Verify the connection is secure. This is crucial for unicodePwd modifications.
            //// This is a conceptual check; the LdapConnection should have been established securely.
            //if (!ldapConnection.LdapConnection.SessionOptions.SecureSocketLayer)
            //{
            //    // Log error: "Password operations require a secure LDAP connection (SSL/TLS or StartTLS)."
            //    // Depending on your error handling strategy, you might throw an exception here.
            //    return false;
            //}


            var attributeModification = new DirectoryAttributeModification
            {
                Name = attributeName,
                Operation = operation
            };
            if (value is byte[] bytes)
            {
                attributeModification.Add(bytes);

            }
            else if (value is string str)
            {
                attributeModification.Add(str);

            }

            var modifyRequest = new ModifyRequest(DN, attributeModification);
            ModifyResponse modifyResponse = (ModifyResponse)ldapConnection.LdapConnection.SendRequest(modifyRequest);

            // Check the result code from the LDAP server
            if (modifyResponse.ResultCode == ResultCode.Success)
            {


                return true;
            }
            else
            {
                // Log the specific LDAP error:
                // string errorMessage = modifyResponse.ErrorMessage;
                // ResultCode resultCode = modifyResponse.ResultCode;
                // Log.Error($"Failed to set password for {userDistinguishedName}. LDAP Error: {resultCode} - {errorMessage}");

                // Your original code caught TargetInvocationException HResult -2146232828 (0x80131604)
                // and returned false. You would need to map specific LDAP ResultCodes to this behavior
                // if you know what underlying ADSI error that HResult represented.
                // For example, password policy violations often return ConstraintViolation or UnwillingToPerform.
                // if (resultCode == ResultCode.ConstraintViolation || resultCode == ResultCode.UnwillingToPerform)
                // {
                //     // This might be equivalent to the scenario where your Invoke call failed with that HResult
                //     return false;
                // }
                return false; // General failure to set password
            }



        }


        public void RefreshCache()
        {
            throw new NotImplementedException();
            //UnderlyingEntry.RefreshCache();

        }
        public void Rename(string newName)
        {
            // 1. Validate the new name
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException("The new name cannot be null or empty.", nameof(newName));
            }

            // 2. Extract the parent DN and current RDN
            var match = Regex.Match(this.DN, @"(?<!\\),");
            if (!match.Success)
            {
                throw new InvalidOperationException("Cannot rename a top-level entry.");
            }
            string parentDn = this.DN.Substring(match.Index + 1);
            string oldRdn = this.DN.Substring(0, match.Index);

            // 3. Construct the new RDN, preserving the RDN type (e.g., "CN", "OU")
            string rdnType = oldRdn.Substring(0, oldRdn.IndexOf('='));
            string newRdn = rdnType + "=" + newName;

            // 4. Call the generalized method. The parent DN stays the same.
            PerformModifyDN(newRdn, parentDn);

           // 5. AFTER the rename succeeds, update the displayName by reusing
    // the existing private Invoke method.
    if (!Invoke("displayName", DirectoryAttributeOperation.Replace, newName))
    {
        // The rename itself succeeded, but the secondary displayName update failed.
        // We log this as a warning because the primary goal was met.
        Loggers.ActiveDirectoryLogger.Warning(
            "Object {DN} was renamed successfully, but updating its displayName attribute failed.", this.DN);
    }
        }

        public void MoveTo(IDirectoryEntry newParent)
        {
            // 1. Validate the new parent
            if (newParent == null || string.IsNullOrWhiteSpace(newParent.DN))
            {
                throw new ArgumentNullException(nameof(newParent), "New parent entry and its DN cannot be null.");
            }

            // 2. Extract the current RDN
            var match = Regex.Match(this.DN, @"(?<!\\),");
            if (!match.Success)
            {
                throw new InvalidOperationException("Cannot move a top-level entry.");
            }
            string rdn = this.DN.Substring(0, match.Index);

            // 3. Call the generalized method. The RDN stays the same.
            PerformModifyDN(rdn, newParent.DN);
        }

        /// <summary>
        /// Performs a generic ModifyDN operation, which is the underlying protocol request
        /// for both moving and renaming an entry.
        /// </summary>
        /// <param name="newRdn">The new Relative Distinguished Name for the object.</param>
        /// <param name="newParentDn">The new parent container's Distinguished Name.</param>
        private void PerformModifyDN(string newRdn, string newParentDn)
        {
            // Store the original DN for logging and cache clearing
            string oldDn = this.DN;

            // Connect to the directory server
            using var connection = SecureLdapConnector.Connect(this.Directory.ConnectionSettings);
            if (connection == null)
            {
                throw new Exception("Failed to connect to the directory server to perform the move/rename operation.");
            }

            // Create and configure the ModifyDNRequest
            var request = new ModifyDNRequest
            {
                DistinguishedName = oldDn,
                NewParentDistinguishedName = newParentDn,
                NewName = newRdn,
                DeleteOldRdn = true
            };

            try
            {
                // Send the request
                connection.LdapConnection.SendRequest(request);

                // On success, update the object's state to its new DN
                this.DN = newRdn + "," + newParentDn;

                // Invalidate the cache for both the old and new DNs
                DirectoryCache.Clear(oldDn);
                DirectoryCache.Clear(this.DN);
            }
            catch (DirectoryException ex)
            {
                // Log the specific error and re-throw to notify the caller
                Loggers.ActiveDirectoryLogger.Error(ex, "ModifyDN operation failed for {OldDN}", oldDn);
                throw;
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    UnderlyingEntry?.Dispose();
                }

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

    /// <summary>
    /// A collection of directory entries that uses System.DirectoryServices.Protocols
    /// to interact with the LDAP server.
    /// </summary>
    public class LdapDirectoryEntries : IDirectoryEntries
    {
        private readonly string _parentDn;
        private readonly IActiveDirectoryContext _directory;

        public LdapDirectoryEntries(string parentDn, IActiveDirectoryContext directory)
        {
            _parentDn = parentDn;
            _directory = directory;
        }

        /// <summary>
        /// Gets an enumerator that iterates through the child entries.
        /// </summary>
        /// <returns>An IEnumerator for the collection of child IDirectoryEntry objects.</returns>
        public IEnumerator GetEnumerator()
        {
            using var connection = SecureLdapConnector.Connect(_directory.ConnectionSettings);
            if (connection == null) yield break;

            var request = new SearchRequest(
                _parentDn,
                "(objectClass=*)", // Filter to find all objects
                System.DirectoryServices.Protocols.SearchScope.OneLevel, // Search only the immediate children
                null // Request all attributes
            );

            var response = (SearchResponse)connection.LdapConnection.SendRequest(request);

            foreach (SearchResultEntry entry in response.Entries)
            {
                yield return new LdapDirectoryEntry(entry.DistinguishedName, _directory);
            }
        }

        /// <summary>
        /// Adds a new entry to the directory under the current parent DN.
        /// </summary>
        /// <param name="name">The RDN of the new object (e.g., "CN=New User").</param>
        /// <param name="schemaClassName">The primary object class of the new entry (e.g., "user").</param>
        /// <returns>The newly created directory entry.</returns>
        public IDirectoryEntry Add(string name, string schemaClassName)
        {
            string newEntryDn = name + "," + _parentDn;
            var request = new AddRequest(newEntryDn, schemaClassName);

            using var connection = SecureLdapConnector.Connect(_directory.ConnectionSettings);
            connection.LdapConnection.SendRequest(request);

            return new LdapDirectoryEntry(newEntryDn, _directory);
        }

        /// <summary>
        /// Finds a child entry by its name (RDN value).
        /// </summary>
        /// <param name="name">The name of the entry to find (e.g., "CN=John Doe").</param>
        /// <returns>The found directory entry.</returns>
        public IDirectoryEntry Find(string name)
        {
            return Find(name, null);
        }

        /// <summary>
        /// Finds a child entry by its name and optionally by its schema class.
        /// </summary>
        /// <param name="name">The name of the entry to find (e.g., "CN=John Doe").</param>
        /// <param name="schemaClassName">The schema class to filter by (optional).</param>
        /// <returns>The found directory entry.</returns>
        public IDirectoryEntry Find(string name, string schemaClassName)
        {
            string filter = "(&(objectClass=*)(|(cn=" + name + ")(ou=" + name + ")))";
            if (!string.IsNullOrEmpty(schemaClassName))
                filter = "(&(objectClass=" + schemaClassName + ")(|(cn=" + name + ")(ou=" + name + ")))";


            using var connection = SecureLdapConnector.Connect(_directory.ConnectionSettings);
            var request = new SearchRequest(_parentDn, filter, System.DirectoryServices.Protocols.SearchScope.OneLevel, null);
            var response = (SearchResponse)connection.LdapConnection.SendRequest(request);

            if (response.Entries.Count > 0)
            {
                return new LdapDirectoryEntry(response.Entries[0].DistinguishedName, _directory);
            }
            return null;
        }

        /// <summary>
        /// Removes a child entry from the directory.
        /// </summary>
        /// <param name="entry">The directory entry to remove.</param>
        public void Remove(IDirectoryEntry entry)
        {
            var request = new DeleteRequest(entry.DN);
            using var connection = SecureLdapConnector.Connect(_directory.ConnectionSettings);
            connection.LdapConnection.SendRequest(request);
        }

    }
    public class AttributeSchemaInfo
    {
        public string AtttributeName { get; set; }
        public string AttributeSyntax { get; set; } // e.g., "2.5.5.12"
        public int OMSyntax { get; set; }          // e.g., 64
        public string OMObjectClass { get; set; }   // Dotted OID string if OMSyntax is 127
        public byte[] OMObjectClassBytes { get; set; }
        public bool IsSingleValued { get; set; }
    }

}
