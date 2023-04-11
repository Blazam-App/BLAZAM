
using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Extensions;
using BLAZAM.Common.Helpers;
using BLAZAM.Common.Models.Database;
using BLAZAM.Common.Models.Database.Permissions;
using BLAZAM.Logger;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.TeamFoundation.Work.WebApi.Exceptions;
using Newtonsoft.Json.Linq;
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
        public AppEvent? OnModelDeleted{ get; set; }

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
        /// <summary>
        /// Actions to perform during <see cref="CommitChanges"/>
        /// </summary>
        protected List<Func<bool>> CommitActions { get; set; } = new();

        private DirectoryEntry? directoryEntry;

        protected SearchResult? searchResult;

        protected AppDatabaseFactory DbFactory;
        protected IApplicationUserState? CurrentUser { get; set; }

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

        protected IActiveDirectoryContext Directory;
        private bool hasUnsavedChanges = false;



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
        public void EnsureDirectoryEntry()
        {
            if (DirectoryEntry is null)
            {
                FetchDirectoryEntry();
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
        private bool _isDeleted = false;
        public virtual bool IsDeleted
        {
            get
            {

                return GetProperty<bool>("isdeleted")||_isDeleted;
            }
            private set { _isDeleted = value; }

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
            parentOUToMoveTo.EnsureDirectoryEntry();
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
            if (CurrentUser == null) return false;

            if (CurrentUser.IsSuperAdmin) return true;
            if (DN == null)
            {
                Loggers.ActiveDirectryLogger.Error("The directory object " + ADSPath
                    + " did not load a distinguished name.");
                return false;
            }
            var baseSearch = CurrentUser.DirectoryUser?.PermissionMappings
                .Where(pm => DN.Contains(pm.OU)).OrderByDescending(pm => pm.OU.Length);

            if (baseSearch == null)
            {
                Loggers.ActiveDirectryLogger.Error("The active user state for " + DN + " could not" +
                    "be found in the application cache.");
                return false;
            }
            try
            {
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
            }catch(Exception ex)
            {
                Loggers.SystemLogger.Error(ex.Message);
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
        public virtual bool CanReadAnyCustomFields
        {
            get
            {
                return HasPermission(p => p.Where(pm =>
             pm.AccessLevels.Any(al => al.FieldMap.Any(om =>
             om.CustomField != null &&
             om.FieldAccessLevel.Level > FieldAccessLevels.Deny.Level
             ))));
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
        public virtual bool CanRename { get => HasActionPermission(ObjectActions.Rename); }
        /// <inheritdoc/>
        public virtual bool CanMove { get => HasActionPermission(ObjectActions.Move); }
        /// <inheritdoc/>
        public virtual bool CanCreate { get => HasActionPermission(ObjectActions.Create); }


        protected virtual bool HasActionPermission(ObjectAction action)
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
        public virtual bool CanDelete { get => HasActionPermission(ObjectActions.Delete); }

        /// <inheritdoc/>
        public virtual bool HasUnsavedChanges
        {
            get => hasUnsavedChanges;

            set
            {
                hasUnsavedChanges = value;
                OnModelChanged?.Invoke();
            }
        }
        protected ADSettings? DirectorySettings { get; private set; }

        /// <inheritdoc/>
        public virtual bool CanReadField(IActiveDirectoryField field)
        {
            if(field is ActiveDirectoryField)
            {
                return HasPermission(p => p.Where(pm =>
              pm.AccessLevels.Any(al => al.FieldMap.Any(om =>
              om.Field?.FieldName == field.FieldName &&
              om.FieldAccessLevel.Level > FieldAccessLevels.Deny.Level
              ))),
              p => p.Where(pm =>
              pm.AccessLevels.Any(al => al.FieldMap.Any(om =>
              om.Field?.FieldName == field.FieldName &&
              om.FieldAccessLevel.Level == FieldAccessLevels.Deny.Level
              )))
              );
            }
            else if(field is CustomActiveDirectoryField)
            {
                return HasPermission(p => p.Where(pm =>
              pm.AccessLevels.Any(al => al.FieldMap.Any(om =>
              om.CustomField?.FieldName == field.FieldName &&
              om.FieldAccessLevel.Level > FieldAccessLevels.Deny.Level
              ))),
              p => p.Where(pm =>
              pm.AccessLevels.Any(al => al.FieldMap.Any(om =>
              om.CustomField?.FieldName == field.FieldName &&
              om.FieldAccessLevel.Level == FieldAccessLevels.Deny.Level
              )))
              );
            }
            throw new ApplicationException("The field provided is invalid");
           

        }

        /// <inheritdoc/>
        public virtual bool CanEditField(IActiveDirectoryField field)
        {
            if (field is ActiveDirectoryField)
            {
                return HasPermission(p => p.Where(pm =>
                   pm.AccessLevels.Any(al => al.FieldMap.Any(om =>
                   om.Field?.FieldName == field.FieldName &&
                   om.FieldAccessLevel.Level > FieldAccessLevels.Read.Level
                   ))),
                   p => p.Where(pm =>
                   pm.AccessLevels.Any(al => al.FieldMap.Any(om =>
                   om.Field?.FieldName == field.FieldName &&
                   om.FieldAccessLevel.Level == FieldAccessLevels.Deny.Level
                   )))
                   );

            }
            else if (field is CustomActiveDirectoryField)
            {
                return HasPermission(p => p.Where(pm =>
                    pm.AccessLevels.Any(al => al.FieldMap.Any(om =>
                    om.CustomField?.FieldName == field.FieldName &&
                    om.FieldAccessLevel.Level > FieldAccessLevels.Read.Level
                    ))),
                    p => p.Where(pm =>
                    pm.AccessLevels.Any(al => al.FieldMap.Any(om =>
                    om.CustomField?.FieldName == field.FieldName &&
                    om.FieldAccessLevel.Level == FieldAccessLevels.Deny.Level
                    )))
                    );

            }
            throw new ApplicationException("The field provided is invalid");

           

        }

        /// <inheritdoc/>
        protected virtual async Task Parse(DirectoryEntry? directoryEntry, SearchResult? searchResult, IActiveDirectoryContext directory)
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
            CurrentUser = Directory.CurrentUser;
            DbFactory = directory.Factory;

            DirectorySettings = await DbFactory.CreateDbContext().ActiveDirectorySettings.FirstOrDefaultAsync();


        }

        /// <inheritdoc/>
        public virtual async Task Parse(DirectoryEntry result, IActiveDirectoryContext directory) => await Parse(result, null, directory);


        private void DirectoryEntry_Disposed(object? sender, EventArgs e)
        {
            //Not utilized
        }

        /// <inheritdoc/>
        public virtual async Task Parse(SearchResult result, IActiveDirectoryContext directory) => await Parse(null, result, directory);

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
                    //Existing Active Directory Entry
                    if (DirectoryEntry == null)
                    {
                        Loggers.ActiveDirectryLogger.Error("The directory entry for an existing " +
                            " entry is somehow missing on commit.");
                        throw new ApplicationException("DirectoryEntry is null");
                    }
                    foreach (var p in NewEntryProperties)
                    {


                        if (!DirectoryEntry.Properties.Contains(p.Key)
                            || DirectoryEntry.Properties[p.Key].Value?.Equals(p.Value) != true)
                        {
                            if (p.Value == null
                        || (p.Value is string strValue && strValue.IsNullOrEmpty())
                        || (p.Value is DateTime dateValue && dateValue == DateTime.MinValue))
                            {
                                
                                    DirectoryEntry.Properties[p.Key].Clear();

                            }
                            else
                            {
                                DirectoryEntry.Properties[p.Key].Value = p.Value;

                            }
                        }
                    }

                    DirectoryEntry.CommitChanges();

                }
                else
                {
                    DirectoryEntry.CommitChanges();

                    if (DirectoryEntry == null)
                    {
                        Loggers.ActiveDirectryLogger.Error("The directory entry for new entry " + DN +
                            " is somehow missing on commit.");
                        throw new ApplicationException("DirectoryEntry is null");
                    }
                    foreach (var p in NewEntryProperties)
                    {
                        if (p.Value == null
                            || (p.Value is string strValue && strValue.IsNullOrEmpty())
                            || (p.Value is DateTime dateValue && dateValue == DateTime.MinValue)) continue;
                        DirectoryEntry.Properties[p.Key].Value = p.Value;
                        DirectoryEntry.CommitChanges();

                    }
                }




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
                        IsDeleted = true;
                        OnModelDeleted?.Invoke();
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
                FetchDirectoryEntry();
            else
                DirectoryEntry?.RefreshCache();

            OnModelChanged?.Invoke();

        }

        private void FetchDirectoryEntry()
        {
            if (SearchResult is null) throw new ArgumentNullException(nameof(SearchResult));

            DirectoryEntry = SearchResult?.GetDirectoryEntry();
        }

        public virtual T? GetCustomProperty<T>(string propertyName)
        {
            try
            {
                return GetProperty<T>(propertyName);
            }
            catch
            {
                return default(T);
            }
        }

        public virtual DateTime? GetDateTimeProperty(string propertyName)
        {
            try
            {
                var com = GetProperty<object>(propertyName);
                return com.AdsValueToDateTime().Value;
            }
            catch
            {
                return null;
            }
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
                    FetchDirectoryEntry();
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
                return GetValue<string>(propertyName).ToString();


            }
            catch (ArgumentOutOfRangeException)
            {
                return "";
            }
            catch (Exception)
            {

                return "";
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
        public virtual void SetCustomProperty(string propertyName, object? value) => SetProperty(propertyName, value);  
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
                    
                    if (value == null || (value is string strValue && strValue == ""))
                    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        NewEntryProperties[propertyName] = null;
                        HasUnsavedChanges = true;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    }
                    else
                    {
                        SetNewProperty(propertyName, value);
                    }
                    
                }
                else
                {
#pragma warning disable CS8601 // Possible null reference assignment.
                    SetNewProperty(propertyName, value);
#pragma warning restore CS8601 // Possible null reference assignment.
                }
                

            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }

        private void SetNewProperty(string propertyName, object? value)
        {
            if (!value.Equals(DirectoryEntry?.Properties[propertyName]?.Value))
            {
                NewEntryProperties[propertyName] = value;

                HasUnsavedChanges = true;
                OnModelChanged?.Invoke();
            }
            else if (NewEntryProperties.ContainsKey(propertyName))
            {
                NewEntryProperties.Remove(propertyName);
                if (NewEntryProperties.Count < 1)
                    HasUnsavedChanges = false;
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
