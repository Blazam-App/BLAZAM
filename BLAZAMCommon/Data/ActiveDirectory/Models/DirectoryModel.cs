
using BLAZAM.Common.Data.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data.Database;
using BLAZAM.Common.Data.Services;
using BLAZAM.Common.Helpers;
using BLAZAM.Common.Models.Database;
using BLAZAM.Common.Models.Database.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Services.Common;
using System.Collections.Generic;
using System.Data;
using System.DirectoryServices;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace BLAZAM.Common.Data.ActiveDirectory.Models
{

    public class DirectoryModel : IDirectoryModel
    {
        /// <summary>
        /// The base uri to reach this directory model's search result page
        /// </summary>
        public virtual string SearchUri
        {
            get
            {
                return "/search/" + CanonicalName;
            }
        }


        public AppEvent? OnModelChanged { get; set; }

        public AppEvent<IDirectoryModel>? OnDirectoryModelRenamed { get; set; }
        public AppEvent? OnModelCommited { get; set; }
        public List<DirectoryModelChange> Changes
        {
            get
            {
                List<DirectoryModelChange> changes = new();
                foreach (var prop in NewEntryProperties)
                {
                    object currentValue = null;
                    try
                    {
                        currentValue = DirectoryEntry.Properties[prop.Key].Value;
                    }
                    catch
                    {

                    }
                    if ((currentValue == null && prop.Value != null) || !currentValue.Equals(prop.Value))
                        changes.Add(new DirectoryModelChange()
                        {
                            Field = prop.Key,
                            OldValue = DirectoryEntry.Properties[prop.Key].Value,
                            NewValue = prop.Value
                        });
                }
                return changes;

            }
        }
        protected List<Func<bool>> CommitActions { get; set; } = new();
        private DirectoryEntry directoryEntry;
        public SearchResult searchResult;
        protected DatabaseContext Context;
        protected IApplicationUserStateService UserStateService { get; set; }

        bool _newEntry = false;
        public bool NewEntry
        {
            get => _newEntry; set
            {
                _newEntry = value;



            }
        }

        public Dictionary<string, object> NewEntryProperties { get; set; } = new();

        protected IActiveDirectory Directory;

        public bool IsNewDirectoryEntry(IADOrganizationalUnit ou)
        {
            directoryEntry = new DirectoryEntry(ou.DN);
            return false;
        }

        public bool Invoke(string method, object?[]? args = null)
        {
            try
            {
                var changedDirectoryEntry = DirectoryEntry.Invoke(method, args);

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

        public DirectoryEntry DirectoryEntry
        {
            get
            {

                /* if (directoryEntry == null)
                 {
                     directoryEntry = searchResult.GetDirectoryEntry();
                     try
                     {
                         if (directoryEntry.SchemaClassName == null)
                         {
                             directoryEntry = null;
                         }
                     }
                     catch
                     {
                         directoryEntry = null;
                         return null;
                     }
                     directoryEntry.UsePropertyCache = true;

                 }*/

                return directoryEntry;
            }
            set
            {
                directoryEntry = value;
            }
        }

        public ActiveDirectoryObjectType ObjectType
        {
            get
            {
                if (Classes.Contains("top"))
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
                        return typeof(DirectoryModel);
                }
            }
        }

        protected SearchResult SearchResult
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
                    return DirectoryEntry.Path;
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
                if (IsDeleted && cn.Contains("DEL:"))
                    return cn.Substring(0, cn.IndexOf("DEL:")).Replace("\n", "");
                return cn;
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
        /// <summary>
        /// If this object is deleted, this was the last <see cref="IADOrganizationalUnit"/> it was under
        /// </summary>
        public virtual IADOrganizationalUnit? LastKnownParent
        {
            get
            {
                var parentDN = GetStringProperty("lastknownparent");
                return Directory.OUs.FindOuByDN(parentDN);
            }

        }
        public virtual bool IsDeleted
        {
            get
            {

                return GetProperty<bool>("isdeleted");
            }

        }
        public virtual DateTime? LastChanged
        {
            get
            {
                var timeUTC = GetProperty<DateTime?>("whenChanged");

                return DateTime.Parse(timeUTC.Value.ToString("MM/dd/yyyy HH:mm:ssZ"));
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

        public virtual List<string>? Classes
        {
            get
            {
                try
                {
                    return SearchResult.Properties["objectclass"].Cast<string>().ToList();
                }
                catch (ArgumentOutOfRangeException)
                {
                    return null;
                }
            }
            set
            {
                DirectoryEntry.Properties["objectclass"].Value = value;

            }
        }

        public virtual bool MoveTo(IADOrganizationalUnit parentOUToMoveTo)
        {
            this.DirectoryEntry.MoveTo(parentOUToMoveTo.DirectoryEntry);

            return true;
        }

        public virtual string? OU { get => DirectoryTools.DnToOu(ADSPath); }

        public async Task<IADOrganizationalUnit?> GetParent()
        {
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
        protected virtual bool HasPermission(Func<IEnumerable<PermissionMap>, IEnumerable<PermissionMap>> allowSelector, Func<IEnumerable<PermissionMap>, IEnumerable<PermissionMap>>? denySelector = null)
        {
            if (UserStateService.CurrentUserState != null)
            {

                if (UserStateService.CurrentUserState.IsSuperAdmin) return true;

                var baseSearch = UserStateService.CurrentUserState?.DirectoryUser?.PermissionMappings.Where(pm => this.DN.Contains(pm.OU)).OrderByDescending(pm => pm.OU.Length);


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



        public virtual bool CanRename { get => HasActionPermission(ActionAccessFlags.Rename); }

        public virtual bool CanMove { get => HasActionPermission(ActionAccessFlags.Move); }

        public virtual bool CanCreate { get => HasActionPermission(ActionAccessFlags.Create); }


        protected virtual bool HasActionPermission(ActionAccessFlag action)
        {
            return HasPermission(p => p.Where(pm =>
               pm.AccessLevels.Any(al => al.ActionMap.Any(am =>
              am.AllowOrDeny && am.ObjectAction.ActionAccessFlagId == action.ActionAccessFlagId &&
              am.ObjectType == ObjectType
               ))),
               p => p.Where(pm =>
               pm.AccessLevels.Any(al => al.ActionMap.Any(am =>
              !am.AllowOrDeny && am.ObjectAction.ActionAccessFlagId == action.ActionAccessFlagId &&
              am.ObjectType == ObjectType
               )))
               );
        }

        public virtual bool CanDelete { get => HasActionPermission(ActionAccessFlags.Delete); }
        public virtual bool HasUnsavedChanges { get; set; } = false;
        protected ADSettings? DirectorySettings { get; private set; }

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
            if (UserStateService.CurrentUserState.IsSuperAdmin) return true;
            var possibleReads = UserStateService.CurrentUserState.DirectoryUser.PermissionMappings.Where(pm => this.DN.Contains(pm.OU) && pm.AccessLevels.Any(al => al.FieldMap.Any(om => om.Field.FieldName == field.FieldName && om.FieldAccessLevel.Level > FieldAccessLevels.Deny.Level))).OrderByDescending(pm => pm.OU.Length).ToList();
            var possibleDenys = UserStateService.CurrentUserState.DirectoryUser.PermissionMappings.Where(pm => this.DN.Contains(pm.OU) && pm.AccessLevels.Any(al => al.FieldMap.Any(om => om.Field.FieldName == field.FieldName && om.FieldAccessLevel.Level == FieldAccessLevels.Deny.Level))).OrderByDescending(pm => pm.OU.Length).ToList();
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
            return false;

        }

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
            if (UserStateService.CurrentUserState.IsSuperAdmin) return true;
            var possibleReads = UserStateService.CurrentUserState.DirectoryUser.PermissionMappings.Where(pm => this.DN.Contains(pm.OU) && pm.AccessLevels.Any(al => al.FieldMap.Any(om => om.Field.FieldName == field.FieldName && om.FieldAccessLevel.Level > FieldAccessLevels.Read.Level))).OrderByDescending(pm => pm.OU.Length).ToList();
            var possibleDenys = UserStateService.CurrentUserState.DirectoryUser.PermissionMappings.Where(pm => this.DN.Contains(pm.OU) && pm.AccessLevels.Any(al => al.FieldMap.Any(om => om.Field.FieldName == field.FieldName && om.FieldAccessLevel.Level == FieldAccessLevels.Deny.Level))).OrderByDescending(pm => pm.OU.Length).ToList();
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
            return false;

        }

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
            Context = await directory.Factory.CreateDbContextAsync();
            using (var context = await directory.Factory.CreateDbContextAsync())
            {
                DirectorySettings = await context.ActiveDirectorySettings.FirstOrDefaultAsync();
            }

        }

        public virtual async Task Parse(DirectoryEntry result, IActiveDirectory directory) => await Parse(result, null, directory);


        private void DirectoryEntry_Disposed(object? sender, EventArgs e)
        {
            var test = 1;
        }

        public virtual async Task Parse(SearchResult result, IActiveDirectory directory) => await Parse(null, result, directory);
        public virtual async Task<DirectoryChangeResult> CommitChangesAsync()
        {
            return await Task.Run(() =>
            {
                return CommitChanges();
            });
        }

        public virtual DirectoryChangeResult CommitChanges()
        {
            try
            {

                if (!NewEntry)
                {

                    /*
                    var propEnum = DirectoryEntry.Properties.GetEnumerator();
                    while (propEnum.MoveNext())
                    {
                        try
                        {
                            var value = propEnum.Value;
                            if (value is string val && val == "")
                            {
                                DirectoryEntry.Properties[propEnum.Key.ToString()].Clear();
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    */
                    foreach (var p in NewEntryProperties)
                    {
                        if (p.Value == null || (p.Value is string strValue && strValue.IsNullOrEmpty())) continue;
                        if (!DirectoryEntry.Properties.Contains(p.Key) || !DirectoryEntry.Properties[p.Key].Value.Equals(p.Value))
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
                        DirectoryEntry.Parent.Children.Remove(DirectoryEntry);
                        break;
                    default:
                        throw new ApplicationException("Deleting objects other than users is not supported yet.");


                }

            }
            catch (UnauthorizedAccessException ex)
            {

            }
        }
        public virtual void DiscardChanges()
        {
            DirectoryEntry = null;
            HasUnsavedChanges = false;
            NewEntryProperties = new();
            if (SearchResult != null)
                DirectoryEntry = SearchResult.GetDirectoryEntry();
            else
                DirectoryEntry.RefreshCache();

            OnModelChanged?.Invoke();

        }


        protected virtual T GetProperty<T>(string propertyName)
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
        /// Retrieves the requested property value from either the cached SearchResult
        /// or by actively polling Active Directory for the entire object.
        /// The object is cached for future calls.
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
                if (SearchResult.Properties.Contains(propertyName))
                    return (T)SearchResult.Properties[propertyName][0];
                else
                {
                    DirectoryEntry = SearchResult.GetDirectoryEntry();

                }
            }
            try
            {
                if (NewEntryProperties.ContainsKey(propertyName.ToLower()))
                    return (T)NewEntryProperties[propertyName.ToLower()];
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
                return (T)DirectoryEntry.Properties[propertyName]?.Value;
            }
            catch (ArgumentException)
            {
                var temp = DirectoryEntry.Properties[propertyName];
                var temp2 = (T)temp.Value;
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
                object[] rawValue;
                try
                {
                    rawValue = GetValue<object[]>(propertyName);
                }
                catch (InvalidCastException)
                {
                    rawValue = new object[] { GetValue<string>(propertyName) };
                }
                if (rawValue != null)
                {
                    foreach (object o in rawValue)
                    {
                        values.Add(o.ToString());
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
        /// Sets an attribute value. Note that this change is uncommited, CommitChanges()
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
                    var oldValue = DirectoryEntry.Properties[propertyName].Value;
                    if (value == null || (value is string strValue && strValue == ""))
                    {
                        NewEntryProperties[propertyName] = null;
                    }
                    else
                    {
                        NewEntryProperties[propertyName] = value;
                    }
                    //Changes.Add(new DirectoryModelChange { Field = propertyName, OldValue = oldValue, NewValue = value });
                }
                else
                {
                    NewEntryProperties[propertyName] = value;
                }
                HasUnsavedChanges = true;
                OnModelChanged?.Invoke();

            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }
        public virtual bool Rename(string newName)
        {
            DirectoryEntry.Rename("cn=" + newName);
            OnDirectoryModelRenamed?.Invoke(this);
            return true;
        }
        protected virtual void RemoveFromListProperty(string propertyName, object? value)
        {
            try
            {
                var before = DirectoryEntry.Properties[propertyName].Value;
                DirectoryEntry.Properties[propertyName].Remove(value);
                var after = DirectoryEntry.Properties[propertyName].Value;

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
                DirectoryEntry.Properties[propertyName].Add(value);
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
            return obj is DirectoryModel model &&
                   DN == model.DN;
        }
        /*
        public override int GetHashCode()
        {
            return DN?.GetHashCode();
        }
        */
    }
}
