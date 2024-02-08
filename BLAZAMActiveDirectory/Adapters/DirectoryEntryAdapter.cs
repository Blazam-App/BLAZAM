using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Helpers;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Logger;
using BLAZAM.Session.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.Data;
using System.DirectoryServices;
using System.Linq;
using System.Reflection;
using MudBlazor;
using System.DirectoryServices.ActiveDirectory;
using BLAZAM.Jobs;
using BLAZAM.Localization;
using Microsoft.Extensions.Localization;

namespace BLAZAM.ActiveDirectory.Adapters
{

    public class DirectoryEntryAdapter : IDirectoryEntryAdapter
    {


        public virtual string SearchUri
        {
            get
            {
                return "/search/" + CanonicalName;
            }
        }


        public AppEvent? OnModelChanged { get; set; }


        public AppEvent<IDirectoryEntryAdapter>? OnDirectoryModelRenamed { get; set; }


        public AppEvent? OnModelCommited { get; set; }


        public AppEvent? OnModelDeleted { get; set; }


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
                    if (currentValue == null && prop.Value != null || currentValue != null && !currentValue.Equals(prop.Value))
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
        protected List<JobStep> CommitSteps { get; set; } = new();

        /// <summary>
        /// Actions to perform during <see cref="CommitChanges"/> but to happen after an initial commit, for new entries
        /// </summary>
        protected List<JobStep> PostCommitSteps { get; set; } = new();

        /// <summary>
        /// The .NET <see cref="DirectoryEntry"/>  underlying object
        /// </summary>
        private DirectoryEntry? directoryEntry;

        protected SearchResult? searchResult;

        protected IAppDatabaseFactory DbFactory => Directory.Factory;

        protected IApplicationUserState? CurrentUser => Directory.CurrentUser;

        bool _newEntry = false;
        public bool NewEntry
        {
            get => _newEntry; set
            {
                _newEntry = value;



            }
        }

        public Dictionary<string, object> NewEntryProperties { get; set; } = new();

        public IActiveDirectoryContext Directory { get; private set; }
        private bool hasUnsavedChanges = false;




        public bool Invoke(string method, object?[]? args = null)
        {
            try
            {
                EnsureDirectoryEntry();
                if (DirectoryEntry != null)
                {
                    var changedDirectoryEntry = DirectoryEntry?.Invoke(method, args);

                    return true;
                }
                return false;

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
                    if (Classes.Contains("printQueue"))
                    {
                        return ActiveDirectoryObjectType.Printer;
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



        public virtual string DN
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

                return GetProperty<bool>("isdeleted") || _isDeleted;
            }
            private set { _isDeleted = value; }

        }
        public virtual bool HasChildren
        {
            get
            {
                var children = DirectoryEntry.Children;
                var entries = children.Encapsulate();
                return entries.Count > 0;
            }
        }
        public virtual DateTime? LastChanged
        {
            get
            {
                return GetDateTimeProperty("whenChanged");
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
                if (!IsDeleted)
                {
                    return GetStringListProperty("objectClass");
                }
                else
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
            }
            set
            {
                if (DirectoryEntry != null)
                    DirectoryEntry.Properties["objectclass"].Value = value;
                else
                    Loggers.ActiveDirectryLogger.Error("Error setting objectClass for " + DN);

            }
        }

        public virtual void MoveTo(IADOrganizationalUnit parentOUToMoveTo)
        {
            CommitSteps.Add(new Jobs.JobStep("Move to OU", (JobStep? step) =>
              {
                  parentOUToMoveTo.EnsureDirectoryEntry();
                  if (parentOUToMoveTo.DirectoryEntry != null)
                  {
                      DirectoryEntry?.MoveTo(parentOUToMoveTo.DirectoryEntry);

                      return true;
                  }
                  return false;
              }));

            HasUnsavedChanges = true;
        }

        public virtual string? OU { get => DirectoryTools.DnToOu(DN); }

        public IADOrganizationalUnit? GetParent()
        {
            if (DirectoryEntry == null || DirectoryEntry.Parent == null) return null;

            var parent = new ADOrganizationalUnit();

            parent.Parse(Directory, DirectoryEntry.Parent);
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
                    + " did not load a distinguished name." + " {@Error}", new ApplicationException());
                return false;
            }
            var baseSearch = CurrentUser.PermissionMappings
                .Where(pm => DN.Contains(pm.OU)).OrderByDescending(pm => pm.OU.Length);

            if (baseSearch == null)
            {
                Loggers.ActiveDirectryLogger.Error("The active user state for " + DN + " could not" +
                    "be found in the application cache." + " {@Error}", new ApplicationException());
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
            }
            catch (Exception ex)
            {
                Loggers.SystemLogger.Error(ex.Message);
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



        public virtual bool CanRename { get => HasActionPermission(ObjectActions.Rename); }

        public virtual bool CanMove { get => HasActionPermission(ObjectActions.Move); }

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

        public virtual bool CanDelete { get => HasActionPermission(ObjectActions.Delete); }


        public virtual bool HasUnsavedChanges
        {
            get => hasUnsavedChanges;

            set
            {
                hasUnsavedChanges = value;
                OnModelChanged?.Invoke();
            }
        }
        protected ADSettings? DirectorySettings => Directory.ConnectionSettings;

        public bool IsExpanded { get; set; }

        public bool IsSelected { get; set; }

        public virtual IEnumerable<IDirectoryEntryAdapter> CachedChildren { get; protected set; } = new List<IDirectoryEntryAdapter>();
        public virtual IEnumerable<IDirectoryEntryAdapter> Children => new List<IDirectoryEntryAdapter>();

        public virtual bool CanReadField(IActiveDirectoryField field)
        {
            if (field is ActiveDirectoryField)
            {
                return HasPermission(p => p.Where(pm =>
              pm.AccessLevels.Any(al => al.FieldMap.Any(om =>
              om.Field?.FieldName == field.FieldName &&
              om.FieldAccessLevel.Level > FieldAccessLevels.Deny.Level &&
              om.ObjectType == ObjectType
              ))),
              p => p.Where(pm =>
              pm.AccessLevels.Any(al => al.FieldMap.Any(om =>
              om.Field?.FieldName == field.FieldName &&
              om.FieldAccessLevel.Level == FieldAccessLevels.Deny.Level &&
              om.ObjectType == ObjectType
              )))
              );
            }
            else if (field is CustomActiveDirectoryField)
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


        public virtual void Parse(IActiveDirectoryContext directory, DirectoryEntry? directoryEntry = null, SearchResult? searchResult = null)
        {
            Directory = directory;

            if (searchResult != null)
                SearchResult = searchResult;

            if (directoryEntry != null)
            {
                DirectoryEntry = directoryEntry;

                //DirectoryEntry.UsePropertyCache = true;
            }



        }





        public virtual async Task<IJob> CommitChangesAsync(IJob? commitJob = null)
        {
            return await Task.Run(() =>
            {
                return CommitChanges(commitJob);
            });
        }
        private IJobStep CommitStep => new JobStep("Save directory entry", (step) =>
                {
            DirectoryEntry?.CommitChanges();

            return true;
        });

        public virtual IJob CommitChanges(IJob? commitJob = null)
        {
            try
            {
                commitJob ??= new Job
                {
                    Name = "Commit Changes",
                    User = CurrentUser?.AuditUsername
                };

               

                IJobStep? propertyStep;
                if (!NewEntry)
                {
                    //Existing Active Directory Entry
                    if (DirectoryEntry == null)
                    {
                        Loggers.ActiveDirectryLogger.Error("The directory entry for an existing " +
                            " entry is somehow missing on commit." + " {@Error}", new ApplicationException("DirectoryEntry is null"));
                        throw new ApplicationException("DirectoryEntry is null");
                    }
                    foreach (var p in NewEntryProperties)
                    {
                        propertyStep = new JobStep("Set AD attributes", (step) =>
                         {



                             if (!DirectoryEntry.Properties.Contains(p.Key)
                                 || DirectoryEntry.Properties[p.Key].Value?.Equals(p.Value) != true)
                             {
                                 if (p.Value == null
                             || p.Value is string strValue && strValue.IsNullOrEmpty()
                             || p.Value is DateTime dateValue && dateValue == DateTime.MinValue)
                                 {

                                     DirectoryEntry.Properties[p.Key].Clear();


                                 }
                                 else
                                 {
                                     DirectoryEntry.Properties[p.Key].Value = p.Value;

                                 }
                             }

                             DirectoryEntry.CommitChanges();
                             return true;
                         });
                        commitJob.Steps.Add(propertyStep);

                    }
                    commitJob.Steps.Add(CommitStep);

                }
                else
                {

                    if (DirectoryEntry == null)
                    {
                        Loggers.ActiveDirectryLogger.Error("The directory entry for new entry " + DN +
                            " is somehow missing on commit." + " {@Error}", new ApplicationException("DirectoryEntry is null"));
                        throw new ApplicationException("DirectoryEntry is null");
                    }
                    foreach (var p in NewEntryProperties)
                    {
                        if (p.Value == null
                               || p.Value is string strValue && strValue.IsNullOrEmpty()
                               || p.Value is DateTime dateValue && dateValue == DateTime.MinValue) continue;
                        propertyStep = new JobStep("Set " + p.Key, (step) =>
                        {
                            DirectoryEntry.Properties[p.Key].Value = p.Value;
                            return true;
                        });
                        commitJob.Steps.Add(propertyStep);
                    }
                }


                //Inject custom commit steps
                foreach (var step in CommitSteps)
                {
                    commitJob.Steps.Add(step);


                }
                if (!NewEntry)
                {
                    if (PostCommitSteps.Count > 0)
                    {
                        foreach (var step in PostCommitSteps)
                        {
                            commitJob.Steps.Add(step);
                        }
                       //commitJob.Steps.Add(CommitStep);

                    }
                }
                commitJob.Steps.Add(CommitStep);
                if (NewEntry)
                {
                    if (PostCommitSteps.Count > 0)
                    {
                        foreach (var step in PostCommitSteps)
                        {
                            commitJob.Steps.Add(step);
                        }
                        commitJob.Steps.Add(CommitStep);

                    }
                }


                var result = commitJob.Run();



                if (result == true)
                {
                    HasUnsavedChanges = false;

                    OnModelCommited?.Invoke();
                }

                return commitJob;
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
                    case ActiveDirectoryObjectType.Printer:
                    case ActiveDirectoryObjectType.Computer:
                        DirectoryEntry?.Parent.Children.Remove(DirectoryEntry);
                        IsDeleted = true;
                        OnModelDeleted?.Invoke();
                        break;
                    default:
                        throw new ApplicationException("Deleting that object type is not supported yet.");


                }

            }
            catch (UnauthorizedAccessException ex)
            {
                throw new ApplicationException("The application directory user does not " +
                    "have permission to delete entries", ex);
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectryLogger.Error(ex.Message + " {@Error}", ex);
            }
        }


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
                return default;
            }
        }

        public virtual DateTime? GetDateTimeProperty(string propertyName)
        {
            try
            {

                var com = GetProperty<object>(propertyName);
                return com?.AdsValueToDateTime();
            }
            catch
            {
                return null;
            }
        }

        protected virtual List<T?> GetNonReplicatedProperty<T>(string propertyName)
        {
            var list = new List<T?>();
            var dcs = new List<DomainController>(Directory.DomainControllers);
            foreach (var dc in dcs)
            {
                try
                {
                    if (dc.IsPingable())
                    {
                        var searcher = dc.GetDirectorySearcher();
                        searcher.Filter = "(distinguishedName=" + this.DN + ")";
                        searcher.ClientTimeout = TimeSpan.FromMilliseconds(500);
                        searcher.ServerTimeLimit = TimeSpan.FromMilliseconds(500);
                        var searchResult = searcher.FindOne();
                        var value = searchResult.GetDirectoryEntry().Properties[propertyName].Value;

                        list.Add((T)value);
                    }
                }
                catch
                {
                    list.Add(default(T));
                }
            }
            return list;
        }


        protected virtual T? GetProperty<T>(string propertyName)
        {
            try
            {
                return GetValue<T>(propertyName);
            }
            catch
            {
                return default;
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
                    if (NewEntryProperties.ContainsKey(propertyName))
                        return (T)NewEntryProperties[propertyName];
                }
                catch (InvalidCastException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectryLogger.Error("Unexpected error while getting property value. {@Error}", ex);
                }



                return default;

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
                return default;

            }

            try
            {
                if (DirectoryEntry != null && DirectoryEntry.Properties.Contains(propertyName))
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
                return default;
            }
            return default;

        }

        protected virtual string? GetStringProperty(string propertyName)
        {
            try
            {
                return GetValue<string>(propertyName)?.ToString();


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

                    if (value == null || value is string strValue && strValue == "")
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
            if (value != null && !value.Equals(DirectoryEntry?.Properties[propertyName]?.Value))
            {
                NewEntryProperties[propertyName] = value;

                HasUnsavedChanges = true;
                OnModelChanged?.Invoke();
            }
            else if (value == null && NewEntryProperties.ContainsKey(propertyName))
            {
                NewEntryProperties.Remove(propertyName);
                if (NewEntryProperties.Count < 1)
                    HasUnsavedChanges = false;
            }
        }


        public virtual bool Rename(string newName)
        {
            newName = newName.Replace(",", "\\,");
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
