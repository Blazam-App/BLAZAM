

using AngleSharp.Dom;
using BLAZAM.ActiveDirectory.Data;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.ActiveDirectory.Searchers;
using BLAZAM.ActiveDirectory.Services;
using BLAZAM.Common.Data;
using BLAZAM.Database.Models;
using BLAZAM.Helpers;
using BLAZAM.Logger;
using System.DirectoryServices.Protocols;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class LdapDirectoryEntry : IDirectoryEntry
    {
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
            Dictionary<string, object?> entryAttributes = new();
            entryAttributes["distinguishedname"] = dn;
            var cached = DirectoryCache.GetEntryCache(dn);
            if (cached == null)
            {
                DirectoryCache.SetEntryCache(dn, entryAttributes);
            }
        }
        public LdapDirectoryEntry(SearchResultEntry sre, IActiveDirectoryContext directory)
        {
            DN = sre.DistinguishedName;
            Directory = directory;
            Dictionary<string, object?> entryAttributes = new();
            var cache = DirectoryCache.GetEntryCache(DN);
            if (cache == null)
            {
                cache = new EntryCache(entryAttributes);
            }
            ProcessAttributes(cache, sre);


        }
        /// <summary>
        /// The full LDAP path to this directory entry, constructed from the connection context and the entry's distinguished name (DN).
        /// </summary>
        public string Path
        {
            get
            {  // Construct the full LDAP path using the connection context and the object's DN.
               // This replaces the dependency on the old UnderlyingEntry.Path.
                if (Directory?.ConnectionSettings != null && !string.IsNullOrEmpty(DN))
                {
                    return $"LDAP://{Directory.ConnectionSettings.ServerAddress}:{Directory.ConnectionSettings.ServerPort}/{DN}";
                }
                // Fallback to returning just the DN if context is unavailable.
                return DN;
            }
        }
        private bool _isNew = false;
        // NEW: Private constructor for creating a new, in-memory entry.
        private LdapDirectoryEntry(string proposedDn, IActiveDirectoryContext directory, bool isNew)
        {
            DN = proposedDn;
            Directory = directory;
            _isNew = isNew;
            // Immediately place a new attribute dictionary into the static DirectoryCache for this proposed DN.
            var initialAttributes = new Dictionary<string, object?> { { "distinguishedname", proposedDn } };

            DirectoryCache.SetEntryCache(proposedDn, initialAttributes);
        }
        /// <summary>
        /// Creates a new directory entry in memory. Call CommitChanges() to save it to the directory.
        /// </summary>
        /// <param name="objectType">The type of object to create (e.g., User, Group).</param>
        /// <param name="name">The name of the new object (e.g., "John Doe", "Sales Team").</param>
        /// <param name="parent">The parent container entry where this object will be created.</param>
        /// <param name="directory">The Active Directory context.</param>
        /// <returns>An uncommitted LdapDirectoryEntry instance.</returns>
        public static LdapDirectoryEntry Create(ActiveDirectoryObjectType objectType, string name, string parentDn, IActiveDirectoryContext directory)
        {
            // Determine RDN prefix (e.g., CN, OU)
            string rdnPrefix = objectType == ActiveDirectoryObjectType.OU ? "OU" : "CN";
            string rdn = $"{rdnPrefix}={name.EscapeLdapSearchFilter().Replace(",", "\\,")}";
            string proposedDn = $"{rdn},{parentDn}";

            var newEntry = new LdapDirectoryEntry(proposedDn, directory, true);

            // Pre-populate mandatory and default attributes into the DirectoryCache
            newEntry.SetDefaultAttributes(objectType, name);

            return newEntry;
        }

        // NEW: Helper to set default attributes based on object type.
        private void SetDefaultAttributes(ActiveDirectoryObjectType objectType, string name)
        {
            // Sets the essential attributes to define an object's name and class,
            // plus safe, standard defaults for behavioral attributes like UAC and Group Type.
            switch (objectType)
            {
                case ActiveDirectoryObjectType.User:
                    SetPropertyValue("objectClass", new[] { "top", "person", "organizationalPerson", "user" });
                    SetPropertyValue(ActiveDirectoryFields.CanonicalName.FieldName, name);

                    // Set UAC to 514, which is the bitwise combination of:
                    // 512 (NORMAL_ACCOUNT) | 2 (ACCOUNTDISABLE)
                    SetPropertyValue("userAccountControl", "514");
                    break;

                case ActiveDirectoryObjectType.Group:
                    SetPropertyValue("objectClass", new[] { "top", "group" });
                    SetPropertyValue(ActiveDirectoryFields.CanonicalName.FieldName, name);
                    SetPropertyValue(ActiveDirectoryFields.Name.FieldName, name);
                    SetPropertyValue("sAMAccountName", name);

                    // Set GroupType to -2147483644, the value for a Global Security Group.
                    SetPropertyValue("groupType", "-2147483644");
                    break;

                case ActiveDirectoryObjectType.Computer:
                    SetPropertyValue("objectClass", new[] { "top", "person", "organizationalPerson", "user", "computer" });
                    SetPropertyValue(ActiveDirectoryFields.CanonicalName.FieldName, name);
                    SetPropertyValue(ActiveDirectoryFields.Name.FieldName, name);
                    // The sAMAccountName for a computer account must end with a '$'.
                    SetPropertyValue("sAMAccountName", name + "$");

                    // Set UAC to 4096, the value for a WORKSTATION_TRUST_ACCOUNT.
                    SetPropertyValue("userAccountControl", "4096");
                    break;

                case ActiveDirectoryObjectType.Contact:
                    SetPropertyValue("objectClass", new[] { "top", "person", "organizationalPerson", "contact" });
                    SetPropertyValue(ActiveDirectoryFields.CanonicalName.FieldName, name);
                    SetPropertyValue(ActiveDirectoryFields.Name.FieldName, name);
                    SetPropertyValue("displayName", name);
                    break;

                case ActiveDirectoryObjectType.OU:
                    SetPropertyValue("objectClass", new[] { "top", "organizationalUnit" });
                    // The RDN for an Organizational Unit is 'ou'.
                    SetPropertyValue(ActiveDirectoryFields.OU.FieldName, name);
                    break;

                default:
                    throw new ArgumentException("Unsupported object type for creation.", nameof(objectType));
            }
        }

        public string? NativeGuid => GetPropertyValue("nativeGuid")?.ToString();

        public void SetPropertyValue(string propertyName, object? value)
        {
            // NEW: If the object is a new in-memory placeholder, update its entry in the DirectoryCache.
            if (_isNew)
            {
                var cacheEntry = DirectoryCache.GetEntryCache(this.DN);
                if (cacheEntry != null)
                {
                    cacheEntry.Attributes[propertyName.ToLower()] = value;
                }
                return;
            }
            Invoke(propertyName, DirectoryAttributeOperation.Replace, value);
        }

        public void RemovePropertyValue(string propertyName, object? value)
        {
            Invoke(propertyName, DirectoryAttributeOperation.Delete, value);
        }
        public void AddPropertyValue(string propertyName, object? value)
        {
            Invoke(propertyName, DirectoryAttributeOperation.Add, value);
        }
        public void ClearPropertyValue(string propertyName)
        {
            Invoke(propertyName, DirectoryAttributeOperation.Delete, null);
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
                return existingCache?.Attributes.ContainsKey(propertyName.ToLower()) == true;

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
            // This avoids a pointless LDAP search for an object that doesn't exist yet.
            if (_isNew)
            {
                var cacheEntry = DirectoryCache.GetEntryCache(this.DN);
                cacheEntry.Attributes.TryGetValue(propertyName.ToLower(), out var value);
                return value;
            }
            return Search(propertyName);
        }

        public List<object?> GetNonReplicatedPropertyValue(string propertyName)
        {
            // This avoids a pointless LDAP search for an object that doesn't exist yet.
            if (_isNew)
            {
                var cacheEntry = DirectoryCache.GetEntryCache(this.DN);
                cacheEntry.Attributes.TryGetValue(propertyName.ToLower(), out var value);
                return [value];
            }
            var results = SearchNonReplicated(propertyName);
            return [];
        }

        public string? DN
        {
            get;
            set;
        }


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
                        GetAttribute(attributeName, existingCache);
                        if (!existingCache.Attributes.ContainsKey(attributeName.ToLower()))
                        {
                            existingCache.Attributes[attributeName.ToLower()] = null;
                        }
                    }
                    return existingCache.Attributes[attributeName.ToLower()];
                }
            }
            else
            {
                existingCache = new(new());
            }
            if (_isNew) return null;
            Loggers.ActiveDirectoryLogger.Debug("Creating ldapConnection in LdapDirectoryEntry {@DirectoryNotNull}", Directory != null && Directory.ConnectionSettings != null);

            GetAllAttributes(existingCache);

            if (!existingCache.Attributes.ContainsKey("isdeleted"))
            {
                existingCache.Attributes["isdeleted"] = false;
            }
            if (!existingCache.Attributes.ContainsKey(attributeName.ToLower()))
            {
                GetAttribute(attributeName, existingCache);
                if (!existingCache.Attributes.ContainsKey(attributeName.ToLower()))
                {
                    existingCache.Attributes[attributeName.ToLower()] = null;
                }
            }

            _propertiesCollected = true;

            return existingCache.Attributes[attributeName.ToLower()]; // Attribute not found on the object or no value



        }


        public virtual List<object?> SearchNonReplicated(string attributeName)
        {
            var existingCache = DirectoryCache.GetEntryCache(DN);

            if (existingCache == null)
            {
                existingCache = new(new());
            }

            var list = new List<object?>();

            var dcs = new List<string>(Directory.DomainControllers);

            Parallel.ForEach(dcs, dc =>
            {
                try
                {

                    // Search request to get ALL user attributes for the specified DN
                    SearchRequest attributesSearchRequest = new SearchRequest(
                        DN,                                 // The DN of the object
                        "(objectClass=*)",                  // Filter to match the object
                        SearchScope.Base, // Target a specific object
                        attributeName                                // Request all user attributes
                    );


                    var searchResponse = SendRequestAndGetResponse<SearchResponse>(attributesSearchRequest, dc);



                    SearchResultEntry entry = searchResponse.Entries[0];

                    var convertedValue = ConvertSingleValue(entry.Attributes[attributeName], attributeName);
                    if (convertedValue != null)
                    {
                        list.Add(convertedValue);
                    }



                }
                catch
                {

                }
            });

            return list;
        }
        /// <summary>
        /// Retrieves the specified attribute for the current distinguished name (DN) and processes it using the
        /// provided cache.
        /// </summary>
        /// <remarks>This method performs a base-level directory search for the specified DN and processes
        /// the resulting attributes. If an existing cache is provided, it is updated with the retrieved
        /// values.</remarks>
        /// <param name="attributeName">The name of the attribute to retrieve. If null, all attributes are requested.</param>
        /// <param name="existingCache">An optional cache to store or update the retrieved attribute values. Can be null if no caching is required.</param>
        private void GetAttribute(string? attributeName, EntryCache? existingCache)
        {
            // Search request to get ALL user attributes for the specified DN
            SearchRequest attributesSearchRequest = new SearchRequest(
                DN,                                 // The DN of the object
                "(objectClass=*)",                  // Filter to match the object
                SearchScope.Base, // Target a specific object
                attributeName                                // Request all user attributes
            );


            var searchResponse = SendRequestAndGetResponse<SearchResponse>(attributesSearchRequest);



            SearchResultEntry entry = searchResponse.Entries[0];

            ProcessAttributes(existingCache, entry);
        }


        private void GetAllAttributes(EntryCache? existingCache)
        {
            GetAttribute(null, existingCache);
        }
        /// <summary>
        ///  
        /// </summary>
        /// <param name="existingCache"></param>
        /// <param name="entry"></param>
        private void ProcessAttributes(EntryCache? existingCache, SearchResultEntry entry)
        {
            GetNamingContext(this);

            foreach (string currentAttributeLdapName in entry.Attributes.AttributeNames)
            {

                DirectoryAttribute directoryAttribute = entry.Attributes[currentAttributeLdapName];
                GetSchemaInfo(currentAttributeLdapName);

                if (_schemaCache.ContainsKey(currentAttributeLdapName) && _schemaCache[currentAttributeLdapName] == null)
                {
                    // Schema not found for this attribute.
                    // Cache null for this attribute, consistent with original behavior for unresolvable schema.
                    existingCache.Attributes[currentAttributeLdapName.ToLower()] = null;
                    Loggers.ActiveDirectoryLogger.Warning("Warning: Schema not found for attribute '{currentAttributeLdapName}'. It will be cached as null.", currentAttributeLdapName);

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
                        var attrName = currentAttributeLdapName.ToLower();
                        var attEnum = directoryAttribute.GetEnumerator();
                        attEnum.MoveNext();
                        object attr = attEnum.Current;

                        existingCache.Attributes[attrName] = ConvertSingleValue(attr, attrName);
                    }
                    else
                    {
                        List<object> values = new List<object>();
                        foreach (object rawValue in directoryAttribute)
                        {

                            values.Add(ConvertSingleValue(rawValue, currentAttributeLdapName));
                        }
                        existingCache.Attributes[currentAttributeLdapName.ToLower()] = values.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectoryLogger.Warning(ex, "Error converting attribute '{currentAttributeLdapName}'. It will be cached as null.");
                    existingCache.Attributes[currentAttributeLdapName.ToLower()] = null; // Cache null if conversion fails
                }

            }
            DirectoryCache.SetEntryCache(DN, existingCache.Attributes);
        }

        private static void GetNamingContext(LdapDirectoryEntry entry)
        {
            if (_namingContextCache.IsNullOrEmpty())
            {

                lock (_namingContextLock)
                {
                    if (_namingContextCache.IsNullOrEmpty())
                    {
                        // First, find the schema naming context
                        var rootDseRequest = new SearchRequest("", "(objectClass=*)", SearchScope.Base, "schemaNamingContext");
                        var rootDseResponse = entry.SendRequestAndGetResponse<SearchResponse>(rootDseRequest);
                        if (rootDseResponse == null || rootDseResponse.Entries.Count == 0)
                        {
                            throw new AppException("Could not read RootDSE to find schema naming context.");
                        }

                        _namingContextCache = rootDseResponse.Entries[0].Attributes["schemaNamingContext"][0].ToString() ?? String.Empty;
                    }
                }
            }
        }

        private void GetSchemaInfo(string propertyName)
        {
            if (_schemaCache.IsNullOrEmpty())
            {
                lock (_schemaLock)
                {
                    SearchRequest schemaSearchRequest = new SearchRequest(
                       _namingContextCache,
                       $"(&(objectClass=attributeSchema)(cn={propertyName}))", // Filter by specific property name
                       SearchScope.Subtree,
                       "attributeSyntax", "oMSyntax", "isSingleValued", "oMObjectClass", "cn"
                    );

                    try
                    {

                        if (!_schemaCache.ContainsKey(propertyName))
                        {

                            var schemaSearchResponse = SendRequestAndGetResponse<SearchResponse>(schemaSearchRequest);
                            if (schemaSearchResponse != null && schemaSearchResponse.Entries.Count > 0)
                            {
                                foreach (SearchResultEntry entry in schemaSearchResponse.Entries)
                                {
                                    AddSchemaCacheEntry(propertyName, entry);
                                }


                            }
                            else
                            {
                                Loggers.ActiveDirectoryLogger.Information("Warning: Schema not found for attribute {PropertyName}.", propertyName);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        Loggers.ActiveDirectoryLogger.Warning(ex, "Error fetching schema for attribute {propertyName}. Schema will be considered not found.", propertyName);
                    }
                }
            }
            if (!_schemaCache.ContainsKey(propertyName))
            {

                SearchRequest schemaSearchRequest = new SearchRequest(
                    _namingContextCache,
                    $"(&(objectClass=attributeSchema)(ldapDisplayName={propertyName}))",
                    SearchScope.Subtree,
                    "attributeSyntax", "oMSyntax", "isSingleValued", "oMObjectClass", "cn" // "cn" can be useful for debugging
                );

                try
                {

                    var schemaSearchResponse = SendRequestAndGetResponse<SearchResponse>(schemaSearchRequest);
                    if (!_schemaCache.ContainsKey(propertyName))
                    {
                        if (schemaSearchResponse != null && schemaSearchResponse.Entries.Count > 0)
                        {
                            AddSchemaCacheEntry(propertyName, schemaSearchResponse.Entries[0]);
                        }
                        else
                        {
                            // Log: $"Schema not found for attribute '{attributeLdapDisplayName}'."
                            Loggers.ActiveDirectoryLogger.Information("Warning: Schema not found for attribute {PropertyName}.", propertyName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectoryLogger.Warning(ex, "Error fetching schema for attribute {propertyName}. Schema will be considered not found.", propertyName);
                }
            }
        }

        private static void AddSchemaCacheEntry(string propertyName, SearchResultEntry entry)
        {
            var info = new AttributeSchemaInfo
            {
                AtttributeName = propertyName, // Use the name we looked up by
                OMSyntax = int.Parse(entry.Attributes["oMSyntax"][0].ToString()),
                IsSingleValued = bool.Parse(entry.Attributes["isSingleValued"][0].ToString())
            };
            lock (_schemaCache)
            {
                _schemaCache.Add(propertyName, info);
            }
        }

        private static object? ConvertSingleValue(object rawValue, string propertyName)
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
                    if (rawValue is byte[] bytesInt)
                    {
                        // Decode the byte array (which represents a string) into a string first.
                        var stringValue = Encoding.UTF8.GetString(bytesInt);
                        // Then, parse the resulting string to an integer.
                        return int.Parse(stringValue);
                    }
                    // If the rawValue is already a string or a numeric type, this will work.
                    return Convert.ToInt32(rawValue);

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
                    if (DateTime.TryParseExact(timeStr.TrimEnd('Z'),
                        ["yyyyMMddHHmmss.f", "yyyyMMddHHmmss", "yyMMddHHmmss"],
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                        out DateTime dt))
                    {
                        return dt;
                    }
                    // Try with explicit Z for older formats if ParseExact with TrimEnd('Z') fails on some inputs
                    if (DateTime.TryParseExact(timeStr,
                        ["yyyyMMddHHmmss.0Z", "yyyyMMddHHmmssZ", "yyMMddHHmmssZ"],
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
                                schemaInfo.AtttributeName.Equals("lockoutTime", StringComparison.OrdinalIgnoreCase) ||
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
                        // For simplicity, returning as string representation or bytes
                        return Convert.ToBase64String(bytesSD);
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


        public string Name => GetPropertyValue("name")?.ToString();

        public IDirectoryEntry Parent
        {
            get
            {
                return new LdapDirectoryEntry(DN.GetParentDn(), Directory);
            }
        }

        public IDirectoryEntries Children => new LdapDirectoryEntries(DN, Directory);

        public AuthType AuthenticationType
        {
            get
            {
                using var connection = Directory.CheckConnect();
                return connection.LdapConnection.AuthType;
            }
        }
        public bool SslEnabled
        {
            get
            {
                using var connection = Directory.CheckConnect();
                return connection?.LdapConnection.SessionOptions.SecureSocketLayer ?? false;
            }
        }
        public CipherAlgorithmType EncryptionType
        {
            get
            {
                using var connection = Directory.CheckConnect();
                try
                {
                    return connection?.LdapConnection.SessionOptions.SslInformation?.AlgorithmIdentifier ?? CipherAlgorithmType.None;
                }
                catch
                {
                    return CipherAlgorithmType.Null;
                }
            }
        }
        public bool UsePropertyCache { get; set; }



        public void CommitChanges()
        {
            if (_isNew)
            {
                var attributesToCommit = DirectoryCache.GetEntryCache(this.DN)?.Attributes;
                if (attributesToCommit == null)
                {
                    throw new InvalidOperationException("Cannot commit a new entry because its attribute cache is missing.");
                }

                var addRequest = new AddRequest(DN);

                foreach (var attr in attributesToCommit)
                {
                    // distinguishedName is part of the request DN, not an attribute in the payload.
                    if (attr.Key.Equals("distinguishedname", StringComparison.OrdinalIgnoreCase)) continue;
                    if (attr.Key.Equals("cn", StringComparison.OrdinalIgnoreCase)) continue;
                    if (attr.Key.Equals("name", StringComparison.OrdinalIgnoreCase)) continue;

                    var dirAttr = new DirectoryAttribute(attr.Key);
                    if (attr.Value is string strValue) dirAttr.Add(strValue);
                    else if (attr.Value is byte[] byteValue) dirAttr.Add(byteValue);
                    else if (attr.Value is string[] strArray)
                    {
                        foreach (var val in strArray) dirAttr.Add(val);
                    }
                    else if (attr.Value != null) dirAttr.Add(attr.Value.ToString());

                    if (dirAttr.Count > 0)
                        addRequest.Attributes.Add(dirAttr);
                }


                if (addRequest.Attributes.Count == 0) throw new InvalidOperationException("Cannot commit a new entry with no attributes.");

                var response = SendRequestAndGetResponse<AddResponse>(addRequest);
                if (response?.ResultCode == ResultCode.Success)
                {
                    _isNew = false; // The object is now "live".
                    RefreshCache(); // Refresh the cache with any server-side generated attributes.
                }
                else
                {
                    throw new DirectoryException($"Failed to create entry. LDAP Error: {response?.ResultCode} - {response?.ErrorMessage}");
                }
            }
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
        }

        private bool InvokeRemove(object[]? args)
        {
            var value = args[0].ToString();
            if (Invoke("member", DirectoryAttributeOperation.Delete, value))
            {
                DirectoryCache.Clear(value);
                return true;
            }
            return false;
        }

        private bool InvokeAdd(object[]? args)
        {
            var value = args[0].ToString();

            if (Invoke("member", DirectoryAttributeOperation.Add, value))
            {
                DirectoryCache.Clear(value);
                return true;
            }
            return false;
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
            string formattedPassword2 = "\"" + newPassword + "\"";
            byte[] newPasswordBytes2 = Encoding.Unicode.GetBytes(formattedPassword2);

            if (Invoke("unicodePwd", DirectoryAttributeOperation.Replace, newPasswordBytes2))
            {
                return true;
            }
            return false;

        }

        private bool Invoke(string attributeName, DirectoryAttributeOperation operation, object? value = null)
        {


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
            else if (value is int integer)
            {
                attributeModification.Add(integer.ToString());
            }

            var modifyRequest = new ModifyRequest(DN, attributeModification);
            var modifyResponse = SendRequestAndGetResponse<ModifyResponse>(modifyRequest);

            // Check the result code from the LDAP server
            if (modifyResponse?.ResultCode == ResultCode.Success)
            {
                // Invalidate the cache for this entry upon successful modification.
                RefreshCache();
                return true;
            }
            else
            {
                return false; // General failure to set password
            }



        }


        public void RefreshCache()
        {
            // Invalidate the cache for this entry upon successful modification.
            DirectoryCache.Clear(DN);
            _ = Search("cn");

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
            string rdn = this.Rdn();

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
            string? oldDn = this.DN;



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
                SendRequestAndGetResponse<ModifyDNResponse>(request);
                // On success, update the object's state to its new DN
                this.DN = newRdn + "," + newParentDn;

                // Invalidate the cache for both the old and new DNs
                DirectoryCache.Clear(oldDn);
                DirectoryCache.Clear(newParentDn);
                RefreshCache();

            }
            catch (DirectoryException ex)
            {
                // Log the specific error and re-throw to notify the caller
                Loggers.ActiveDirectoryLogger.Error(ex, "ModifyDN operation failed for {OldDN}", oldDn);
                throw;
            }
        }

        /// <summary>
        /// A generic helper method to send a directory request and return the correctly typed response.
        /// This consolidates connection handling and response casting.
        /// </summary>
        /// <typeparam name="T">The expected DirectoryResponse type.</typeparam>
        /// <param name="request">The DirectoryRequest to be sent.</param>
        /// <returns>The resulting DirectoryResponse, cast to the specified type.</returns>
        private T? SendRequestAndGetResponse<T>(DirectoryRequest request, string? serverHostname = null) where T : DirectoryResponse
        {
            using var connection = Directory.GetConnection(serverHostname);
            return (T?)connection?.SendRequest(request);
        }

        public List<LdapDirectoryEntry> GetDomainControllers()
        {
            var domainDn = Directory.ConnectionSettings?.FQDN.FqdnToDN();
            if (string.IsNullOrEmpty(domainDn)) return new();

            string dcContainerDn = $"CN=Domain Controllers,{domainDn}";
            var searchRequest = new SearchRequest(
                dcContainerDn,
                "(objectClass=server)",
                SearchScope.OneLevel,
                "distinguishedName", "name"
            );

            var response = SendRequestAndGetResponse<SearchResponse>(searchRequest);
            var controllers = new List<LdapDirectoryEntry>();
            foreach (SearchResultEntry entry in response.Entries)
            {
                controllers.Add(new LdapDirectoryEntry(entry, Directory));
            }
            return controllers;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {

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
