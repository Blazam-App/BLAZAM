
using Azure;
using BLAZAM.ActiveDirectory.Data;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Services;
using BLAZAM.Helpers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class LdapDirectoryEntry : IDirectoryEntry
    {
        public readonly DirectoryEntry UnderlyingEntry;
        protected IActiveDirectoryContext Directory { get; set; }
        private bool disposedValue;
        public LdapDirectoryEntry(DirectoryEntry underlyingEntry)
        {
            UnderlyingEntry = underlyingEntry;
        }
        public LdapDirectoryEntry(string dn, IActiveDirectoryContext directory)
        {
            DN = dn;
            Directory = directory;
        }

        public string Path { get => UnderlyingEntry.Path; set => UnderlyingEntry.Path = value; }

        public string? NativeGuid => UnderlyingEntry.NativeGuid;

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
            return UnderlyingEntry.Properties.Contains(propertyName);
        }
        public bool PropertyContains(string propertyName, object value)
        {
            return UnderlyingEntry.Properties[propertyName].Contains(value);
        }
        public object? GetPropertyValue(string propertyName)
        {
            return Search(propertyName);
        }

        private string? DN { get; set; }


        private object? Search(string attributeName)
        {

            var existingCache = DirectoryCache.GetEntryCache(DN);
            if (existingCache == null) existingCache = new(new());
            if (existingCache.Attributes.ContainsKey(attributeName.ToLower()))
            {
                return existingCache.Attributes[attributeName.ToLower()];
            }
            using var ldapConnection = SecureLdapConnector.Connect(Directory.ConnectionSettings);
            // Verify the connection is secure. This is crucial for unicodePwd modifications.
            // This is a conceptual check; the LdapConnection should have been established securely.
            if (!ldapConnection.LdapConnection.SessionOptions.SecureSocketLayer)
            {
                // Log error: "Password operations require a secure LDAP connection (SSL/TLS or StartTLS)."
                // Depending on your error handling strategy, you might throw an exception here.
                return null;
            }


            // First, find the schema naming context
            var rootDseRequest = new SearchRequest("", "(objectClass=*)", System.DirectoryServices.Protocols.SearchScope.Base, "schemaNamingContext");
            var rootDseResponse = (SearchResponse)ldapConnection.LdapConnection.SendRequest(rootDseRequest);
            if (rootDseResponse.Entries.Count == 0)
            {
                throw new Exception("Could not read RootDSE to find schema naming context.");
            }
            string schemaNamingContext = rootDseResponse.Entries[0].Attributes["schemaNamingContext"][0].ToString();

            //var schemaInfo = GetSchemaInfo(ldapConnection, schemaNamingContext, attributeName);



            // Search request to get ALL user attributes for the specified DN
            SearchRequest allAttributesSearchRequest = new SearchRequest(
                DN,                                 // The DN of the object
                "(objectClass=*)",                  // Filter to match the object
                System.DirectoryServices.Protocols.SearchScope.Base, // Target a specific object
                null                                // Request all user attributes
            );

            //// Construct a search request for the specific entry and attribute
            //SearchRequest searchRequest = new SearchRequest(
            //    DN, // The DN of the object
            //    $"({attributeName}=*)", // A filter to ensure the attribute exists (can be simplified if you know it exists)
            //                            // More simply, if you just want the object and its attributes, you can use "(objectClass=*)"
            //                            // or a more specific filter if needed.
            //    System.DirectoryServices.Protocols.SearchScope.Base,    // We are targeting a specific object
            //    attributeName        // Specify only the attribute you want
            //);

            SearchResponse searchResponse = (SearchResponse)ldapConnection.LdapConnection.SendRequest(allAttributesSearchRequest);



            SearchResultEntry entry = searchResponse.Entries[0];
            foreach (string currentAttributeLdapName in entry.Attributes.AttributeNames)
            {
                // Skip if already processed (e.g., by a direct cache hit before lock or an alias)

                if (existingCache.Attributes.ContainsKey(currentAttributeLdapName.ToLower()))
                {
                    continue;
                }

                DirectoryAttribute directoryAttribute = entry.Attributes[currentAttributeLdapName];
                AttributeSchemaInfo schemaInfo = GetSchemaInfo(ldapConnection, schemaNamingContext, currentAttributeLdapName);

                if (schemaInfo == null)
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
                    else if (schemaInfo.IsSingleValued)
                    {
                        // Assuming ConvertSingleValue is accessible
                        existingCache.Attributes[currentAttributeLdapName.ToLower()] = ConvertSingleValue(directoryAttribute[0], schemaInfo);
                    }
                    else
                    {
                        List<object> values = new List<object>();
                        foreach (object rawValue in directoryAttribute)
                        {
                            values.Add(ConvertSingleValue(rawValue, schemaInfo));
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
                existingCache.Attributes[attributeName] = null;
            }
            DirectoryCache.SetEntryCache(DN, existingCache.Attributes);
            return existingCache.Attributes[attributeName.ToLower()]; // Attribute not found on the object or no value



        }
        private AttributeSchemaInfo GetSchemaInfo(AppLdapConnection ldapConnection, string schemaNamingContext, string attributeLdapDisplayName)
        {
            SearchRequest schemaSearchRequest = new SearchRequest(
                schemaNamingContext,
                $"(&(objectClass=attributeSchema)(ldapDisplayName={attributeLdapDisplayName}))",
                System.DirectoryServices.Protocols.SearchScope.Subtree,
                "attributeSyntax", "oMSyntax", "isSingleValued", "oMObjectClass", "cn" // "cn" can be useful for debugging
            );

            try
            {
                SearchResponse schemaSearchResponse = (SearchResponse)ldapConnection.LdapConnection.SendRequest(schemaSearchRequest);
                if (schemaSearchResponse.Entries.Count > 0)
                {
                    SearchResultEntry schemaEntry = schemaSearchResponse.Entries[0];
                    return new AttributeSchemaInfo
                    {
                        AtttributeName = attributeLdapDisplayName, // Use the name we looked up by
                        AttributeSyntax = schemaEntry.Attributes["attributeSyntax"][0].ToString(),
                        OMSyntax = int.Parse(schemaEntry.Attributes["oMSyntax"][0].ToString()),
                        IsSingleValued = bool.Parse(schemaEntry.Attributes["isSingleValued"][0].ToString()),
                        OMObjectClass = schemaEntry.Attributes.Contains("omObjectClass") && schemaEntry.Attributes["omObjectClass"][0] is byte[] omocBytes
                                        ? Encoding.UTF8.GetString(omocBytes)
                                        : (schemaEntry.Attributes.Contains("omObjectClass") ? schemaEntry.Attributes["omObjectClass"][0]?.ToString() : null)
                    };
                }
                else
                {
                    // Log: $"Schema not found for attribute '{attributeLdapDisplayName}'."
                    Console.WriteLine($"Warning: Schema not found for attribute '{attributeLdapDisplayName}'.");
                }
            }
            catch (Exception ex)
            {
                // Log: $"Error fetching schema for attribute '{attributeLdapDisplayName}': {ex.Message}"
                Console.WriteLine($"Error fetching schema for attribute '{attributeLdapDisplayName}': {ex.Message}. Schema will be considered not found.");
            }
            return null;
        }
        private object ConvertSingleValue(object rawValue, AttributeSchemaInfo schemaInfo)
        {
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
                            return new System.Security.Principal.SecurityIdentifier(bytesOctet, 0).ToString();
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

        public string Name => GetPropertyValue("name").ToString();

        public IDirectoryEntry Parent
        {
            get
            {
                return new LdapDirectoryEntry(DN, Directory);
            }
        }

        public IDirectoryEntries Children => new AdapterDirectoryEntries(UnderlyingEntry.Children);

        public AuthenticationTypes AuthenticationType { get => UnderlyingEntry.AuthenticationType; set => UnderlyingEntry.AuthenticationType = value; }
        public bool UsePropertyCache { get => UnderlyingEntry.UsePropertyCache; set => UnderlyingEntry.UsePropertyCache = value; }

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

            // Verify the connection is secure. This is crucial for unicodePwd modifications.
            // This is a conceptual check; the LdapConnection should have been established securely.
            if (!ldapConnection.LdapConnection.SessionOptions.SecureSocketLayer)
            {
                // Log error: "Password operations require a secure LDAP connection (SSL/TLS or StartTLS)."
                // Depending on your error handling strategy, you might throw an exception here.
                return false;
            }


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

        public void MoveTo(IDirectoryEntry newParent)
        {
            if (newParent is AdapterDirectoryEntry adapterDirectoryEntry)
            {
                UnderlyingEntry.MoveTo(adapterDirectoryEntry.UnderlyingEntry);

            }
        }

        public void RefreshCache()
        {
            UnderlyingEntry.RefreshCache();

        }

        public void Rename(string newName)
        {
            UnderlyingEntry.Rename(newName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    UnderlyingEntry.Dispose();
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
