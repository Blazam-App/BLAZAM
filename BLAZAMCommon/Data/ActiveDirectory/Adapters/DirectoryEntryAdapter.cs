
using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Helpers;
using BLAZAM.Common.Models.Database;
using BLAZAM.Common.Models.Database.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.DirectoryServices;
using System.Linq;
using System.Reflection;

namespace BLAZAM.Common.Data.ActiveDirectory.Models
{

    public class DirectoryEntryAdapter : IDirectoryEntryAdapter
    {
        /// <inheritdoc/>
        public virtual string SearchUri
        {
            get
            {
                return "/search/" + CanonicalName;
            }
        }

        /// <inheritdoc/>
        public AppEvent? OnModelChanged { get; set; }

        /// <inheritdoc/>
        public AppEvent<IDirectoryEntryAdapter>? OnDirectoryModelRenamed { get; set; }

        /// <inheritdoc/>
        public AppEvent? OnModelCommited { get; set; }

        /// <inheritdoc/>
        public virtual List<AuditChangeLog> Changes
        {
            get
            {
                List<AuditChangeLog> changes = new();
                foreach (var prop in NewEntryProperties)
                {
                    object? currentValue = null;
                    try
                    {
                        currentValue = DirectoryEntry?.Properties[prop.Key].Value;
                    }
                    catch
                    {

                    }
                    if ((currentValue == null && prop.Value != null) || (currentValue != null && !currentValue.Equals(prop.Value)))
                        changes.Add(new AuditChangeLog()
                        {
                            Field = prop.Key,
                            OldValue = DirectoryEntry?.Properties[prop.Key].Value,
                            NewValue = prop.Value
                        });
                }
                return changes;

            }
        }
        protected List<Func<bool>> CommitActions { get; set; } = new();
        private DirectoryEntry? directoryEntry;
        public SearchResult? searchResult;
        protected AppDatabaseFactory DbFactory;
        protected IApplicationUserStateService UserStateService { get; set; }

        bool _newEntry = false;
        public bool NewEntry
        {
            get => _newEntry; set
            {
                _newEntry = value;



            }
        }
        /// <inheritdoc/>
        public Dictionary<string, object> NewEntryProperties { get; set; } = new();

        protected IActiveDirectory Directory;



        /// <inheritdoc/>
        public bool Invoke(string method, object?[]? args = null)
        {
            try
            {
                var changedDirectoryEntry = DirectoryEntry?.Invoke(method, args);

                return true;

            }
            catch (TargetInvocationException ex)
            {
                switch (ex.HResult)
                {
                    case -2146232828:

                        return false;
                }
            }
            return false;
        }
        /// <inheritdoc/>
        public DirectoryEntry? DirectoryEntry
        {
            get
            {
                return directoryEntry;
            }
            set
            {
                directoryEntry = value;
            }
        }
        /// <inheritdoc/>
        public ActiveDirectoryObjectType ObjectType
        {
            get
            {
                if (Classes != null && Classes.Contains("top"))
                {
                    if (Classes.Contains("computer"))
                    {
                        return ActiveDirectoryObjectType.Computer;
                    }
                    if (Classes.Contains("user"))
                    {
                        return ActiveDirectoryObjectType.User;
                    }
                    if (Classes.Contains("group"))
                    {
                        return ActiveDirectoryObjectType.Group;
                    }
                    if (Classes.Contains("organizationalUnit"))
                    {
                        return ActiveDirectoryObjectType.OU;
                    }

                }
                return ActiveDirectoryObjectType.OU;
            }
        }
        /// <inheritdoc/>
        public Type ModelType
        {
            get
            {
                switch (ObjectType)
                {
                    case ActiveDirectoryObjectType.User:
                        return typeof(ADUser);
                    case ActiveDirectoryObjectType.Group:
                        return typeof(ADGroup);
                    case ActiveDirectoryObjectType.Computer:
                        return typeof(ADComputer);
                    case ActiveDirectoryObjectType.OU:
                        return typeof(ADOrganizationalUnit);
                    default:
                        return typeof(DirectoryEntryAdapter);
                }
            }
        }

        protected SearchResult? SearchResult
        {
            get => searchResult;
            set => searchResult = value;
        }

        public virtual string? SamAccountName
        {

            get
            {
                return GetStringProperty("samaccountname");
            }
            set
            {
                SetProperty("samaccountname", value);
            }


        }

        public virtual string? ADSPath
        {

            get
            {
                if (DirectoryEntry == null)
                    return GetStringProperty("adspath");
                else
                    return DirectoryEntry?.Path;
            }
            set
            {
                SetProperty("adspath", value);
            }


        }

        public virtual string? CanonicalName
        {
            get
            {
                var cn = GetStringProperty("cn");
                if (cn != null)
                {
                    if (cn.Contains("DEL:"))
                        return cn.Substring(0, cn.IndexOf("DEL:")).Replace("\n", "");
                    return cn;
                }
                return null;
            }
            set
            {
                SetProperty("cn", value);
            }
        }



        public virtual string? DN
        {
            get
            {
                return GetStringProperty("distinguishedName");
            }
            set
            {
                SetProperty("distinguishedName", value);
            }

        }
        /// <inheritdoc/>
        public virtual DateTime? Created
        {
            get
            {
                return GetProperty<DateTime?>("whenCreated");
            }
            set
            {
                SetProperty("whenCreated", value);
            }

        }

        /// <inheritdoc/>
        public virtual IADOrganizationalUnit? LastKnownParent
        {
            get
            {
                var parentDN = GetStringProperty("lastknownparent");
                return parentDN != null ? Directory.OUs.FindOuByDN(parentDN) : null;
            }

        }
        public virtual bool IsDeleted
        {
            get
            {

                return GetProperty<bool>("isdeleted");
            }

        }
        /// <inheritdoc/>
        public virtual DateTime? LastChanged
        {
            get
            {
                var timeUTC = GetProperty<DateTime?>("whenChanged");
                return timeUTC != null ? DateTime.Parse(timeUTC.Value.ToString("MM/dd/yyyy HH:mm:ssZ")) : null;
            }
            set
            {
                SetProperty("whenChanged", value);
            }

        }

        public virtual byte[]? SID
        {
            get
            {
                return GetProperty<byte[]>("objectSID");
            }
            set
            {
                SetProperty("objectSID", value);
            }

        }

        public override string? ToString()
        {
            return DN;
        }
        /// <inheritdoc/>
        public virtual List<string>? Classes
        {
            get
            {
                try
                {
                    return SearchResult?.Properties["objectclass"].Cast<string>().ToList();
                }
                catch (ArgumentOutOfRangeException)
                {
                    return null;
                }
            }
            set
            {
                if (DirectoryEntry != null)
                    DirectoryEntry.Properties["objectclass"].Value = value;
                else
                    Loggers.ActiveDirectryLogger.Error("Error setting objectClass for " + DN);

            }
        }

        public virtual bool MoveTo(IADOrganizationalUnit parentOUToMoveTo)
        {
            if (parentOUToMoveTo.DirectoryEntry != null)
            {
                DirectoryEntry?.MoveTo(parentOUToMoveTo.DirectoryEntry);

                return true;
            }
            return false;
        }

        public virtual string? OU { get => DirectoryTools.DnToOu(ADSPath); }

        public async Task<IADOrganizationalUnit?> GetParent()
        {
            if (DirectoryEntry == null || DirectoryEntry.Parent == null) return null;

            var parent = new ADOrganizationalUnit();

            await parent.Parse(DirectoryEntry.Parent, Directory);
            return parent;


        }



        /// <summary>
        /// Used to check application user permissions. The selectors provided will authomatically search for this DirectoryModel's
        /// OU. Supplied selectors should only check for AccessLevels.Any(...) that match the permission type requested.
        /// </summary>
        /// <param name="allowSelector"></param>
        /// <param name="denySelector"></param>
        /// <returns></returns>
        protected virtual bool HasPermission(Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>> allowSelector, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>>? denySelector = null)
        {
            if (UserStateService.CurrentUserState != null)
            {

                if (UserStateService.CurrentUserState.IsSuperAdmin) return true;
                if (DN == null)
                {
                    Loggers.ActiveDirectryLogger.Error("The directory object " + ADSPath
                        + " did not load a distinguished name.");
                    return false;
                }
                var baseSearch = UserStateService.CurrentUserState?.DirectoryUser?.PermissionMappings
                    .Where(pm => DN.Contains(pm.OU)).OrderByDescending(pm => pm.OU.Length);

                if (baseSearch == null)
                {
                    Loggers.ActiveDirectryLogger.Error("The active user state for " + DN + " could not" +
                        "be found in the application cache.");
                    return false;
                }
                var possibleReads = allowSelector.Invoke(baseSearch).ToList();
                if (denySelector != null)
                {
                    var possibleDenys = denySelector.Invoke(baseSearch).ToList();

                    if (possibleReads != null && possibleReads.Count > 0)
                    {
                        if (possibleDenys != null && possibleDenys.Count > 0)
                        {
                            foreach (var d in possibleDenys)
                            {
                                if (d.OU.Length > possibleReads.OrderByDescending(r => r.OU.Length).First().OU.Length)
                                    return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    return possibleReads?.Count > 0;
                }
            }
            return false;
        }
        /// <inheritdoc/>
        public virtual bool CanRead
        {
            get
            {
                return HasPermission(p => p.Where(pm =>
                pm.AccessLevels.Any(al =>
                al.ObjectMap.Any(om =>
                om.ObjectType == ObjectType &&
                om.ObjectAccessLevel.Level > ObjectAccessLevels.Deny.Level
                ))),
                p => p.Where(pm =>
                pm.AccessLevels.Any(al =>
                al.ObjectMap.Any(om =>
                om.ObjectType == ObjectType &&
                om.ObjectAccessLevel.Level == ObjectAccessLevels.Deny.Level
                )))
                );
            }

        }
        /// <inheritdoc/>
        public virtual bool CanEdit
        {
            get
            {
                return HasPermission(p => p.Where(pm =>
                pm.AccessLevels.Any(al =>
                 al.ObjectMap.Any(om => om.ObjectType == ObjectType) &&
                al.FieldMap.Any(om =>
               om.FieldAccessLevel.Level > FieldAccessLevels.Read.Level
                ))),
                p => p.Where(pm =>
                pm.AccessLevels.Any(al =>
                al.ObjectMap.Any(om => om.ObjectType == ObjectType) &&
                al.FieldMap.Any(om =>
               om.FieldAccessLevel.Level == FieldAccessLevels.Deny.Level
                )))
                );
            }

        }


        /// <inheritdoc/>
        public virtual bool CanRename { get => HasActionPermission(ActionAccessFlags.Rename); }
        /// <inheritdoc/>
        public virtual bool CanMove { get => HasActionPermission(ActionAccessFlags.Move); }
        /// <inheritdoc/>
        public virtual bool CanCreate { get => HasActionPermission(ActionAccessFlags.Create); }


        protected virtual bool HasActionPermission(ActionAccessFlag action)
        {
            return HasPermission(p => p.Where(pm =>
               pm.AccessLevels.Any(al => al.ActionMap.Any(am =>
              am.AllowOrDeny && am.ObjectAction.Id == action.Id &&
              am.ObjectType == ObjectType
               ))),
               p => p.Where(pm =>
               pm.AccessLevels.Any(al => al.ActionMap.Any(am =>
              !am.AllowOrDeny && am.ObjectAction.Id == action.Id &&
              am.ObjectType == ObjectType
               )))
               );
        }
        /// <inheritdoc/>
        public virtual bool CanDelete { get => HasActionPermission(ActionAccessFlags.Delete); }

        /// <inheritdoc/>
        public virtual bool HasUnsavedChanges { get; set; } = false;
        protected ADSettings? DirectorySettings { get; private set; }

        /// <inheritdoc/>
        public virtual bool CanReadField(ActiveDirectoryField field)
        {
            return HasPermission(p => p.Where(pm =>
               pm.AccessLevels.Any(al => al.FieldMap.Any(om =>
               om.Field.FieldName == field.FieldName &&
               om.FieldAccessLevel.Level > FieldAccessLevels.Deny.Level
               ))),
               p => p.Where(pm =>
               pm.AccessLevels.Any(al => al.FieldMap.Any(om =>
               om.Field.FieldName == field.FieldName &&
               om.FieldAccessLevel.Level == FieldAccessLevels.Deny.Level
               )))
               );

        }

        /// <inheritdoc/>
        public virtual bool CanEditField(ActiveDirectoryField field)
        {
            return HasPermission(p => p.Where(pm =>
               pm.AccessLevels.Any(al => al.FieldMap.Any(om =>
               om.Field.FieldName == field.FieldName &&
               om.FieldAccessLevel.Level > FieldAccessLevels.Read.Level
               ))),
               p => p.Where(pm =>
               pm.AccessLevels.Any(al => al.FieldMap.Any(om =>
               om.Field.FieldName == field.FieldName &&
               om.FieldAccessLevel.Level == FieldAccessLevels.Deny.Level
               )))
               );


        }

        /// <inheritdoc/>
        protected virtual async Task Parse(DirectoryEntry? directoryEntry, SearchResult? searchResult, IActiveDirectory directory)
        {
            Directory = directory;

            if (searchResult != null)
                SearchResult = searchResult;

            if (directoryEntry != null)
            {
                DirectoryEntry = directoryEntry;

                DirectoryEntry.UsePropertyCache = true;
                DirectoryEntry.Disposed += DirectoryEntry_Disposed;
            }

            UserStateService = directory.UserStateService;
            DbFactory = directory.Factory;

            DirectorySettings = await DbFactory.CreateDbContext().ActiveDirectorySettings.FirstOrDefaultAsync();


        }

        /// <inheritdoc/>
        public virtual async Task Parse(DirectoryEntry result, IActiveDirectory directory) => await Parse(result, null, directory);


        private void DirectoryEntry_Disposed(object? sender, EventArgs e)
        {
            //Not utilized
        }

        /// <inheritdoc/>
        public virtual async Task Parse(SearchResult result, IActiveDirectory directory) => await Parse(null, result, directory);

        /// <inheritdoc/>
        public virtual async Task<DirectoryChangeResult> CommitChangesAsync()
        {
            return await Task.Run(() =>
            {
                return CommitChanges();
            });
        }

        /// <inheritdoc/>
        public virtual DirectoryChangeResult CommitChanges()
        {
            try
            {

                if (!NewEntry)
                {
                    if (DirectoryEntry == null)
                    {
                        Loggers.ActiveDirectryLogger.Error("The directory entry for a new " +
                            " entry is somehow missing on commit.");
                        throw new ApplicationException("DirectoryEntry is null");
                    }
                    foreach (var p in NewEntryProperties)
                    {
                        if (p.Value == null || (p.Value is string strValue && strValue.IsNullOrEmpty())) continue;
                        if (!DirectoryEntry.Properties.Contains(p.Key) || DirectoryEntry.Properties[p.Key].Value?.Equals(p.Value) != true)
                            DirectoryEntry.Properties[p.Key].Value = p.Value;
                    }
                    foreach (PropertyValueCollection property in DirectoryEntry.Properties)
                    {
                        var value = property.Value;
                        if (value is string val && val == "")
                        {
                            DirectoryEntry.Properties[property.PropertyName].Clear();
                        }
                    }


                }
                else
                {
                    if (DirectoryEntry == null)
                    {
                        Loggers.ActiveDirectryLogger.Error("The directory entry for " + DN +
                            " is somehow missing on commit.");
                        throw new ApplicationException("DirectoryEntry is null");
                    }
                    foreach (var p in NewEntryProperties)
                    {
                        if (p.Value == null || (p.Value is string strValue && strValue.IsNullOrEmpty())) continue;
                        DirectoryEntry.Properties[p.Key].Value = p.Value;
                    }
                }


                DirectoryEntry.CommitChanges();


                foreach (var action in CommitActions)
                {
                    if (!action.Invoke())
                    {
                        throw new ApplicationException("Error processing Commit Actions");
                    }
                }


                HasUnsavedChanges = false;
                OnModelCommited?.Invoke();
                return new DirectoryChangeResult();
            }
            catch (DirectoryServicesCOMException ex)
            {
                throw new ApplicationException(ex.Message + ex.ExtendedErrorMessage, ex);
            }

        }

        /// <inheritdoc/>
        public virtual void Delete()
        {
            try
            {

                switch (ObjectType)

                {
                    case ActiveDirectoryObjectType.User:
                    case ActiveDirectoryObjectType.OU:
                    case ActiveDirectoryObjectType.Group:
                    case ActiveDirectoryObjectType.Computer:
                        DirectoryEntry?.Parent.Children.Remove(DirectoryEntry);
                        break;
                    default:
                        throw new ApplicationException("Deleting objects other than users is not supported yet.");


                }

            }
            catch (UnauthorizedAccessException ex)
            {
                throw new ApplicationException("The application directory user does not " +
                    "have permission to delete entries", ex);
            }
        }

        /// <inheritdoc/>
        public virtual void DiscardChanges()
        {
            DirectoryEntry = null;
            HasUnsavedChanges = false;
            NewEntryProperties = new();
            if (SearchResult != null)
                DirectoryEntry = SearchResult?.GetDirectoryEntry();
            else
                DirectoryEntry?.RefreshCache();

            OnModelChanged?.Invoke();

        }


        protected virtual T? GetProperty<T>(string propertyName)
        {
            try
            {
                return GetValue<T>(propertyName);
            }
            catch
            {
                return default(T);
            }
        }
        /// <summary>
        /// Retrieves the requested property value from either the cached <see cref="SearchResult"/>
        /// or by actively polling Active Directory for the entire object.
        /// The value is cached for future calls.
        /// </summary>
        /// <typeparam name="T">The value type of the requested attribute</typeparam>
        /// <param name="propertyName">The requested attribute</param>
        /// <returns>The attribute value</returns>
        private T? GetValue<T>(string propertyName)
        {

            if (NewEntry)
            {
                try
                {
                    return (T)NewEntryProperties[propertyName];
                }
                catch (InvalidCastException ex)
                {
                    throw ex;
                }
                catch
                {
                    return default(T);

                }
            }
            if (DirectoryEntry == null)
            {
                if (SearchResult != null && SearchResult.Properties.Contains(propertyName))
                    return (T?)SearchResult?.Properties[propertyName][0];
                else
                {
                    DirectoryEntry = SearchResult?.GetDirectoryEntry();

                }
            }
            try
            {
                if (NewEntryProperties.ContainsKey(propertyName))
                    return (T)NewEntryProperties[propertyName];
            }
            catch (InvalidCastException ex)
            {
                throw ex;
            }
            catch
            {
                return default(T);

            }

            try
            {
                return (T?)DirectoryEntry?.Properties[propertyName].Value;
            }
            catch (ArgumentException)
            {
                var temp = DirectoryEntry?.Properties[propertyName];
                var temp2 = (T?)temp?.Value;
                return temp2;
            }
            catch (InvalidCastException ex)
            {
                throw ex;
            }

            catch
            {
                return default(T);
            }


        }

        protected virtual string? GetStringProperty(string propertyName)
        {
            try
            {
                return GetValue<string>(propertyName)?.ToString();

            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        protected virtual List<string>? GetStringListProperty(string propertyName)
        {
            try
            {
                List<string> values = new List<string>();
                object[]? rawValue = null;
                try
                {
                    rawValue = GetValue<object[]>(propertyName);
                }
                catch (InvalidCastException)
                {
                    //Asked for string list but may have found just a single string
                    string? str = GetValue<string>(propertyName);
                    if (str != null)
                        rawValue = new object[] { str };
                }
                if (rawValue != null)
                {
                    foreach (object o in rawValue)
                    {
                        string? str = o?.ToString();
                        if (str != null)
                            values.Add(str);
                    }
                }
                return values;

            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// Sets an attribute value. Note that this change is uncommited, <see cref="CommitChanges"/>
        /// must be called afterwards for the change to persist.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        protected virtual void SetProperty(string propertyName, object? value)
        {
            if (IsDeleted) throw new ApplicationException("Cannot set values for a deleted entry.");
            try
            {
                if (!NewEntry)
                {
                    var oldValue = DirectoryEntry?.Properties[propertyName]?.Value;
                    if (value == null || (value is string strValue && strValue == ""))
                    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        NewEntryProperties[propertyName] = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    }
                    else
                    {
                        NewEntryProperties[propertyName] = value;
                    }
                    //Changes.Add(new AuditChangeLog { Field = propertyName, OldValue = oldValue, NewValue = value });
                }
                else
                {
#pragma warning disable CS8601 // Possible null reference assignment.
                    NewEntryProperties[propertyName] = value;
#pragma warning restore CS8601 // Possible null reference assignment.
                }
                HasUnsavedChanges = true;
                OnModelChanged?.Invoke();

            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }

        /// <inheritdoc/>
        public virtual bool Rename(string newName)
        {
            DirectoryEntry?.Rename("cn=" + newName);
            OnDirectoryModelRenamed?.Invoke(this);
            return true;
        }
        protected virtual void RemoveFromListProperty(string propertyName, object? value)
        {
            try
            {
                var before = DirectoryEntry?.Properties[propertyName].Value;
                DirectoryEntry?.Properties[propertyName].Remove(value);
                var after = DirectoryEntry?.Properties[propertyName].Value;

                HasUnsavedChanges = true;

            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }

        protected virtual void AddToListProperty(string propertyName, object? value)
        {
            try
            {
                DirectoryEntry?.Properties[propertyName].Add(value);
                HasUnsavedChanges = true;

            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }

        public virtual void Dispose()
        {
            directoryEntry?.Dispose();
            searchResult = null;
        }

        public override bool Equals(object? obj)
        {
            return obj is DirectoryEntryAdapter model &&
                   DN == model.DN;
        }

        public override int GetHashCode()
        {
            if (DN != null)
                return DN.GetHashCode();
            else
                return -1;
        }


    }
}
