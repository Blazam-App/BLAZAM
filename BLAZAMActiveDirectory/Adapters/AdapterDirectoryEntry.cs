
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Helpers;
using System.Collections;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Text;

namespace BLAZAM.ActiveDirectory.Adapters
{
    public class AdapterDirectoryEntry : IDirectoryEntry
    {
        public readonly DirectoryEntry UnderlyingEntry;
        protected IActiveDirectoryContext Directory { get; set; }
        private bool disposedValue;
        public AdapterDirectoryEntry(DirectoryEntry underlyingEntry)
        {
            UnderlyingEntry = underlyingEntry;
        }
        public AdapterDirectoryEntry(DirectoryEntry underlyingEntry, IActiveDirectoryContext directory)
        {
            UnderlyingEntry = underlyingEntry;
            Directory = directory;
        }

        public string Path { get => UnderlyingEntry.Path; set => UnderlyingEntry.Path = value; }

        public string? NativeGuid => UnderlyingEntry.NativeGuid;

        public void SetPropertyValue(string propertyName, object? value)
        {
            UnderlyingEntry.Properties[propertyName].Value = value;
        }
        public void RemovePropertyValue(string propertyName, object? value)
        {
            UnderlyingEntry.Properties[propertyName].Remove(value);
        }
        public void AddPropertyValue(string propertyName, object? value)
        {
            UnderlyingEntry.Properties[propertyName].Add(value);
        }
        public void ClearPropertyValue(string propertyName)
        {
            UnderlyingEntry.Properties[propertyName].Clear();
        }
        public bool ContainsProperty(string propertyName)
        {
            return UnderlyingEntry.Properties.Contains(propertyName);
        }
        public bool PropertyContains(string propertyName,object value)
        {
            return UnderlyingEntry.Properties[propertyName].Contains(value);
        }
        public object? GetPropertyValue(string propertyName)
        {
            return UnderlyingEntry.Properties[propertyName].Value;
        }
        public string SchemaClassName => UnderlyingEntry.SchemaClassName;

        public string Name => UnderlyingEntry.Name;

        public IDirectoryEntry Parent
        {
            get
            {
                return new AdapterDirectoryEntry(UnderlyingEntry.Parent,Directory);
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
            //if (methodName.Equals("SetPassword"))
            //{
            //    SetUserPasswordLdap(args[0].ToString());

            //}
            return UnderlyingEntry.Invoke(methodName, args);
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
            LdapConnection? ldapConnection = null;
            SecureLdapConnector.Connect(Directory.ConnectionSettings,out ldapConnection);
            // Verify the connection is secure. This is crucial for unicodePwd modifications.
            // This is a conceptual check; the LdapConnection should have been established securely.
            if (!ldapConnection.SessionOptions.SecureSocketLayer)
            {
                // Log error: "Password operations require a secure LDAP connection (SSL/TLS or StartTLS)."
                // Depending on your error handling strategy, you might throw an exception here.
                return false;
            }

            try
            {
                var dn = GetPropertyValue("distinguishedName")?.ToString();

                // The password for unicodePwd must be a UTF-16LE encoded string, enclosed in double quotes.

                string formattedPassword = "\"" + newPassword + "a\"";
                byte[] newPasswordBytes = Encoding.Unicode.GetBytes(formattedPassword);
                var attributeClearing = new DirectoryAttributeModification
                {
                    Name = "unicodePwd",
                    Operation = DirectoryAttributeOperation.Replace
                };
                attributeClearing.Add(newPasswordBytes);

                var modifyRequest = new ModifyRequest(dn, attributeClearing);
                ModifyResponse modifyResponse = (ModifyResponse)ldapConnection.SendRequest(modifyRequest);

                // Check the result code from the LDAP server
                if (modifyResponse.ResultCode == ResultCode.Success)
                {

                    string formattedPassword2 = "\"" + newPassword + "\"";
                    byte[] newPasswordBytes2 = Encoding.Unicode.GetBytes(formattedPassword2);
                    // Create the directory attribute modification
                    var attributeModification = new DirectoryAttributeModification
                    {
                        Name = "unicodePwd",
                        Operation = DirectoryAttributeOperation.Replace
                    };
                    attributeModification.Add(newPasswordBytes2);

                    // Create the modify request
                    var modifyRequest2 = new ModifyRequest(dn, attributeModification);

                    //// 5. Add the LDAP_SERVER_POLICY_HINTS_OID control
                    //// This is the crucial part to bypass password history.
                    //// The value '1' indicates to enforce policies with administrative bypass.
                    //byte[] controlData = new byte[] { 48, (byte)132, 0, 0, 0, 3, 2, 1, 1 }; // BER encoded integer '1'
                    //var policyHintsControl = new DirectoryControl(
                    //    "1.2.840.113556.1.4.2239", // LDAP_SERVER_POLICY_HINTS_OID
                    //    controlData,
                    //    true, // IsCritical: true means the operation fails if the server doesn't support the control
                    //    true  // ServerSide: indicates this is a server-side control
                    //);

                    //modifyRequest.Controls.Add(policyHintsControl);

                    // Send the request to the LDAP server
                    ModifyResponse modifyResponse2 = (ModifyResponse)ldapConnection.SendRequest(modifyRequest2);

                    // Check the result code from the LDAP server
                    if (modifyResponse2.ResultCode == ResultCode.Success)
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
                else
                {
                    return false;
                }
            }
            catch (DirectoryOperationException ex)
            {
                // This exception is typically thrown when the LDAP server returns an error ResultCode.
                // The actual response from the server is often in ex.Response.
                // Log.Error($"LDAP operation failed for {userDistinguishedName} (SetPassword): {ex.Response?.ResultCode} - {ex.Response?.ErrorMessage}", ex);

                // Again, map specific ex.Response.ResultCode values if they correspond to your
                // original HResult -2146232828 handling.
                // if (ex.Response != null && (ex.Response.ResultCode == ResultCode.ConstraintViolation || ex.Response.ResultCode == ResultCode.UnwillingToPerform))
                // {
                //     return false;
                // }
                return false;
            }
            catch (LdapException ex) // Handles other LDAP communication errors (e.g., server down, protocol error)
            {
                // Log.Error($"LDAP exception for {userDistinguishedName} (SetPassword): {ex.Message}", ex);
                return false;
            }
            catch (Exception ex) // Catch-all for any other unexpected issues
            {
                // Log.Error($"Unexpected error setting password for {userDistinguishedName}: {ex.Message}", ex);
                return false;
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

    /// <summary>
    /// Defines a contract for a collection of directory entries,
    /// compatible with System.DirectoryServices.DirectoryEntries.
    /// </summary>
    public interface IDirectoryEntries : IEnumerable
    {


        /// <summary>
        /// Creates a new entry in this collection.
        /// </summary>
        /// <param name="name">The name of the new entry.</param>
        /// <param name="schemaClassName">The schema class of the new entry.</param>
        /// <returns>The newly created <see cref="IDirectoryEntry"/>.</returns>
        IDirectoryEntry Add(string name, string schemaClassName);

        /// <summary>
        /// Returns the entry at the specified path in this collection.
        /// </summary>
        /// <param name="name">The name of the entry to find.</param>
        /// <param name="schemaClassName">The schema class of the entry to find. If null, any schema class is searched.</param>
        /// <returns>The <see cref="IDirectoryEntry"/> found.</returns>
        IDirectoryEntry Find(string name, string schemaClassName);

        /// <summary>
        /// Returns the entry at the specified path in this collection.
        /// </summary>
        /// <param name="name">The name of the entry to find.</param>
        /// <returns>The <see cref="IDirectoryEntry"/> found.</returns>
        IDirectoryEntry Find(string name);

        /// <summary>
        /// Removes the specified entry from this collection.
        /// </summary>
        /// <param name="entry">The <see cref="IDirectoryEntry"/> to remove.</param>
        void Remove(IDirectoryEntry entry);
    }


    public class AdapterDirectoryEntries : IDirectoryEntries
    {
        public DirectoryEntries UnderlyingDirectoryEntries { get; private set; }

        public AdapterDirectoryEntries(DirectoryEntries underlyingDirectoryEntries)
        {
            UnderlyingDirectoryEntries = underlyingDirectoryEntries;
        }

        public IDirectoryEntry Add(string name, string schemaClassName)
        {
            return UnderlyingDirectoryEntries.Add(name, schemaClassName).ToIDirectoryEntry();
        }

        public IDirectoryEntry Find(string name, string schemaClassName)
        {
            return UnderlyingDirectoryEntries.Find(name, schemaClassName).ToIDirectoryEntry();
        }

        public IDirectoryEntry Find(string name)
        {
            return UnderlyingDirectoryEntries.Find(name).ToIDirectoryEntry();
        }

        public IEnumerator GetEnumerator()
        {
            return UnderlyingDirectoryEntries.GetEnumerator();
        }

        public void Remove(IDirectoryEntry entry)
        {
            if (entry is DirectoryEntry dEntry)
                UnderlyingDirectoryEntries.Remove(dEntry);
        }
    }
}
