using BLAZAM.ActiveDirectory.Data;
using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Helpers;
using BLAZAM.Jobs;
using BLAZAM.Logger;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MudBlazor;
using System.Data;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace BLAZAM.ActiveDirectory.Adapters
{

    public class DirectoryEntryAdapter : IDirectoryEntryAdapter
    {


        public virtual string SearchUri
        {
            get
            {
                return "/view/" + CanonicalName;
            }
        }
        [JsonIgnore]
        public AppEvent OnModelChanged { get; set; } = new();
        [JsonIgnore]
        public AppEvent OnChangesDiscarded { get; set; } = new();

        [JsonIgnore]
        public AppEvent<IDirectoryEntryAdapter> OnDirectoryModelRenamed { get; set; } = new();

        [JsonIgnore]
        public AppEvent OnModelCommited { get; set; } = new();

        [JsonIgnore]
        public AppEvent OnModelDeleted { get; set; } = new();

        [JsonIgnore]
        public virtual List<AuditChangeLog> Changes
        {
            get
            {
                List<AuditChangeLog> changes = [];




                foreach (var prop in NewEntryProperties)
                {
                    object? currentValue = null;
                    try
                    {
                        currentValue = DirectoryEntry?.Properties[prop.Key].Value;
                    }
                    catch
                    {
                        // If the property does not exist, we can ignore it
                        // This can happen if the property is not set on the DirectoryEntry
                        // or if it is a custom property that does not exist in the DirectoryEntry
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
        protected List<JobStep> CommitSteps { get; set; } = [];

        /// <summary>
        /// Actions to perform during <see cref="CommitChanges"/> but to happen after an initial commit, for new entries
        /// </summary>
        protected List<JobStep> PostCommitSteps { get; set; } = [];




        protected IAppDatabaseFactory DbFactory => Directory.Factory;
        public void SetCurrentUser(ActiveDirectoryUserState user)
        {
            _currentUser = user;
        }
        protected ActiveDirectoryUserState? _currentUser;
        protected ActiveDirectoryUserState? CurrentUser => _currentUser;

        [JsonIgnore]
        public bool NewEntry { get; set; }

        [JsonIgnore]
        public Dictionary<string, object> NewEntryProperties { get; set; } = [];

        [JsonIgnore]
        private IActiveDirectoryContext _directory;
        [JsonIgnore]
        public IActiveDirectoryContext Directory
        {
            get => _directory;
            private set
            {
                if (value.Equals(_directory)) return;
                _directory = value;
                _currentUser = value.CurrentUser;
            }
        }
        private bool _hasUnsavedChanges = false;



        public bool Invoke(string method, object?[]? args = null)
        {
            try
            {
                EnsureDirectoryEntry();
                if (DirectoryEntry != null)
                {
                    DirectoryEntry.Invoke(method, args);

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

        [JsonIgnore]
        public DirectoryEntry? DirectoryEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="MissingDirectoryEntryException">If the DirectoryEntry cannot be retrieved</exception>
        public void EnsureDirectoryEntry()
        {
            if (DirectoryEntry is not null)
            {
                try
                {
                    _ = DirectoryEntry.Path;
                }
                catch (ObjectDisposedException)
                {
                    DirectoryEntry = null;
                }
            }
            if (DirectoryEntry is null)
            {
                FetchDirectoryEntry();
                if (DirectoryEntry == null) throw new MissingDirectoryEntryException("The DirectoryEntry for this object could not be retrieved");

            }
        }

        public virtual ActiveDirectoryObjectType ObjectType
        {
            get
            {
                if (Classes == null || !Classes.Contains("top"))
                    return ActiveDirectoryObjectType.OU;

                return GetObjectTypeFromClasses(Classes);
            }
        }

        private static ActiveDirectoryObjectType GetObjectTypeFromClasses(List<string> classes)
        {
            if (classes.Contains("computer"))
                return ActiveDirectoryObjectType.Computer;
            if (classes.Contains("user"))
                return ActiveDirectoryObjectType.User;
            if (classes.Contains("group"))
                return ActiveDirectoryObjectType.Group;
            if (classes.Contains("organizationalUnit"))
                return ActiveDirectoryObjectType.OU;
            if (classes.Contains("printQueue"))
                return ActiveDirectoryObjectType.Printer;
            if (classes.Contains("contact"))
                return ActiveDirectoryObjectType.Contact;
            if (classes.Contains("msFVE-RecoveryInformation"))
                return ActiveDirectoryObjectType.BitLocker;

            return ActiveDirectoryObjectType.OU;
        }


        protected SearchResult? SearchResult { get; set; }

        public virtual string? ADSPath
        {

            get
            {
                if (DirectoryEntry == null)
                    return GetStringAttribute("adspath");
                else
                    return DirectoryEntry.Path;
            }
            set
            {
                SetAttribute("adspath", value);
            }


        }

        public virtual string? CanonicalName
        {
            get
            {
                var cn = GetStringAttribute(ActiveDirectoryFields.CanonicalName.FieldName);
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
                SetAttribute(ActiveDirectoryFields.CanonicalName.FieldName, value);
            }
        }



        public virtual string? DN
        {
            get
            {
                if (NewEntry)
                {
                    // Regular expression to match everything after the last slash
                    string pattern = @"[^/]+$";

                    Match match = Regex.Match(ADSPath, pattern);

                    if (match.Success)
                    {
                        return match.Value;
                    }
                    else
                    {
                        // Handle the case where no match is found
                        return null;
                    }
                }
                return GetStringAttribute(ActiveDirectoryFields.DistinguishedName.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.DistinguishedName.FieldName, value);
            }

        }

        public virtual DateTime? Created
        {
            get
            {
                return GetAttribute<DateTime?>("whenCreated");
            }
            set
            {
                SetAttribute("whenCreated", value);
            }

        }
        private IADUser? _manager;
        public virtual IADUser? Manager
        {
            get
            {
                if (_manager == null)
                {
                    var dn = GetStringAttribute(ActiveDirectoryFields.Manager.FieldName);

                    var user = Directory.Users.FindUserByDN(dn);
                    _manager = user;
                }
                return _manager;
            }
            set
            {
                _manager = value;
                SetAttribute(ActiveDirectoryFields.Manager.FieldName, value?.DN);
            }

        }


        public virtual IADOrganizationalUnit? LastKnownParent
        {
            get
            {
                var parentDN = GetStringAttribute("lastknownparent");
                return parentDN != null ? Directory.OUs.FindOuByDN(parentDN) : null;
            }

        }
        private bool _isDeleted = false;
        public virtual bool IsDeleted
        {
            get
            {

                return GetAttribute<bool>("isdeleted") || _isDeleted;
            }
            private set { _isDeleted = value; }

        }
        private bool? _cachedHasChildren;
        [JsonIgnore]
        public virtual bool HasChildren
        {
            get
            {
                if (CachedChildren == null)
                {
                    if (_cachedHasChildren == null)
                    {
                        EnsureDirectoryEntry();
                        _cachedHasChildren = DirectoryEntry?.Children.GetEnumerator().MoveNext();
                    }


                    return _cachedHasChildren == true;

                }
                var hasChildren = CachedChildren.Any();
                return hasChildren;
            }
        }
        public virtual DateTime? LastChanged
        {
            get
            {
                return GetDateTimeAttribute("whenChanged");
            }
            set
            {
                SetAttribute("whenChanged", value);
            }

        }

        public virtual byte[]? SID
        {
            get
            {
                return GetAttribute<byte[]>(ActiveDirectoryFields.ObjectSID.FieldName);
            }
            set
            {
                SetAttribute(ActiveDirectoryFields.ObjectSID.FieldName, value);
            }

        }
        public virtual byte[]? Guid
        {
            get
            {
                var bytes = GetAttribute<byte[]>("objectGUID");

                return bytes;
            }
            set
            {
                SetAttribute("objectGUID", value);
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
                    return GetStringListAttribute("objectClass");
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
                    Loggers.ActiveDirectoryLogger.Error("Error setting objectClass for {@DN}", DN);

            }
        }

        public virtual void MoveTo(IADOrganizationalUnit parentOUToMoveTo)
        {
            CommitSteps.Add(new JobStep("Move to OU", (JobStep step) =>
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



        public virtual string? OU { get => DN.DnToOu() ?? ADSPath.DnToOu(); }

        public async Task<IDirectoryEntryAdapter?> GetParentAsync() => await Task.Run(() =>
        {
            return GetParent();
        });

        public IDirectoryEntryAdapter? GetParent()
        {

            EnsureDirectoryEntry();
            if (DirectoryEntry == null || DirectoryEntry.Parent == null) return null;

            var parent = DirectoryEntry.Parent.Encapsulate(Directory);

            return parent;


        }



        /// <summary>
        /// Used to check application user permissions. The selectors provided will authomatically search for this DirectoryModel's
        /// OU. Supplied selectors should only check for AccessLevels.Any(...) that match the permission type requested.
        /// </summary>
        /// <param name="allowSelector"></param>
        /// <param name="denySelector"></param>
        /// <returns></returns>
        protected virtual bool HasPermission(Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>> allowSelector, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>>? denySelector = null, bool nestedSearch = false)
        {
            try
            {
                if (CurrentUser == null) return false;
                if (DN == null)
                {
                    throw new AppException("The directory object " + ADSPath + " did not load a distinguished name.");
                }
                return CurrentUser.HasPermission(DN, allowSelector, denySelector, nestedSearch);
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Error(ex, "Error trying to check entry permissions.");
                return false;
            }
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


        protected virtual bool HasActionPermission(ObjectAction action, ActiveDirectoryObjectType? objectType = null)
        {
            if (CurrentUser == null) return false;
            objectType ??= ObjectType;
            return CurrentUser.HasActionPermission(DN, action, objectType.Value);
        }

        public virtual bool CanDelete { get => HasActionPermission(ObjectActions.Delete); }

        [JsonIgnore]
        public IList<PermissionMapping> InheritedPermissionMappings
        {
            get
            {
                return AppliedPermissionMappings.Where(m => !m.OU.Equals(DN)).ToList();
            }
        }
        [JsonIgnore]
        public IList<PermissionMapping> DirectPermissionMappings
        {
            get
            {

                return AppliedPermissionMappings.Where(m => m.OU.Equals(DN)).ToList();

            }
        }

        private IList<PermissionMapping> _appliedPermissionMappings;
        [JsonIgnore]
        public IList<PermissionMapping> AppliedPermissionMappings
        {
            get
            {
                if (_appliedPermissionMappings == null)
                {
                    using var context = DbFactory.CreateDbContext();
                    _appliedPermissionMappings = context.PermissionMap.Include(m => m.PermissionDelegates).Where(m => DN.Contains(m.OU)).OrderByDescending(m => m.OU.Length).ToList();
                }
                return _appliedPermissionMappings;
            }
        }
        private IList<PermissionMapping> _offspringPermissionMappings;

        [JsonIgnore]
        public IList<PermissionMapping> OffspringPermissionMappings
        {
            get
            {
                if (_offspringPermissionMappings == null)
                {
                    using var context = DbFactory.CreateDbContext();
                    _offspringPermissionMappings = context.PermissionMap.Include(m => m.PermissionDelegates)
                                                                        .Where(m => m.OU.Contains(DN) && m.OU != DN)
                                                                        .OrderByDescending(m => m.OU.Length).ToList();
                }
                return _offspringPermissionMappings;
            }
        }

        public void ClearPermissionCache()
        {
            _offspringPermissionMappings = null;
            _appliedPermissionMappings = null;
        }

        [JsonIgnore]
        public virtual bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;

            set
            {
                _hasUnsavedChanges = value;
                OnModelChanged?.Invoke();
            }
        }
        protected ADSettings? DirectorySettings => Directory.ConnectionSettings;

        [JsonIgnore]
        public bool IsExpanded { get; set; }

        [JsonIgnore]
        public bool IsSelected { get; set; }

        public virtual IEnumerable<IDirectoryEntryAdapter>? CachedChildren { get; set; }
        [JsonIgnore]
        public virtual IEnumerable<IDirectoryEntryAdapter> Children
        {
            get
            {
                if (CachedChildren == null)
                {
                    List<IDirectoryEntryAdapter> directoryEntries = [];
                    EnsureDirectoryEntry();
                    var children = DirectoryEntry.Children;
                    var list = new List<DirectoryEntry>();
                    foreach (DirectoryEntry child in children)
                    {
                        list.Add(child);
                    }
                    Parallel.ForEach<DirectoryEntry>(list, child =>
                    {
                        DirectoryEntryAdapter? thisObject = null;

                        if (child.Properties["objectClass"].Contains("top"))
                        {
                            var objectClass = child.Properties["objectClass"];
                            if (objectClass.Contains("computer"))
                            {
                                thisObject = new ADComputer();
                            }
                            else if (objectClass.Contains("user"))
                            {
                                thisObject = new ADUser();
                            }
                            else if (objectClass.Contains("organizationalUnit"))
                            {
                                thisObject = new ADOrganizationalUnit();
                            }
                            else if (objectClass.Contains("group"))
                            {
                                thisObject = new ADGroup();
                            }
                            else if (objectClass.Contains("printQueue"))
                            {
                                thisObject = new ADPrinter();
                            }

                            else if (objectClass.Contains("msFVE-RecoveryInformation"))
                            {
                                thisObject = new ADBitLockerRecovery();
                            }
                            if (thisObject != null)
                            {
                                thisObject.Parse(directory: Directory, directoryEntry: child);
                                lock (directoryEntries)
                                {
                                    directoryEntries.Add(thisObject);
                                }

                            }

                        }
                    });
                    CachedChildren = directoryEntries.OrderBy(x => x.CanonicalName).ThenBy(x => x.ObjectType);
                }
                return CachedChildren;
            }
        }

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
            throw new AppException("The field provided is invalid");


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
            throw new AppException("The field provided is invalid");



        }


        public virtual void Parse(IActiveDirectoryContext directory, DirectoryEntry? directoryEntry = null, SearchResult? searchResult = null)
        {
            Directory = directory;

            if (searchResult != null)
                SearchResult = searchResult;

            if (directoryEntry != null)
            {
                DirectoryEntry = directoryEntry;
            }



        }





        public virtual async Task<IJob> CommitChangesAsync(IJob? commitJob = null)
        {
            return await Task.Run(() =>
            {
                return CommitChanges(commitJob);
            });
        }
        private IJobStep commitStep => new JobStep("Save directory entry", (step) =>
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
                    User = CurrentUser?.Username
                };



                JobStep? propertyStep;
                var propertyJob = new Job("Set AD attributes", commitJob.User);
                if (!NewEntry)
                {
                    //Existing Active Directory Entry
                    if (DirectoryEntry == null)
                    {
                        Loggers.ActiveDirectoryLogger.Error(new AppException("DirectoryEntry is null"), "The directory entry for an existing " +
                            " entry is somehow missing on commit.");
                        throw new AppException("DirectoryEntry is null");
                    }
                    foreach (var p in NewEntryProperties)
                    {
                        propertyStep = new JobStep("Set AD attribute [" + p.Key + "]", (step) =>
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
                        propertyJob.AddStep(propertyStep);

                    }
                    if (propertyJob.Steps.Count > 0)
                    {
                        commitJob.AddStep(propertyJob);
                    }
                    commitJob.AddStep(commitStep);

                }
                else
                {

                    if (DirectoryEntry == null)
                    {
                        Loggers.ActiveDirectoryLogger.Error(new AppException("DirectoryEntry is null"), "The directory entry for new entry is somehow missing on commit. {@DN}", DN);
                        throw new AppException("DirectoryEntry is null");
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
                        propertyJob.AddStep(propertyStep);
                    }
                    if (propertyJob.Steps.Count > 0)
                    {
                        commitJob.AddStep(propertyJob);
                    }
                }


                //Inject custom commit steps
                foreach (var step in CommitSteps)
                {
                    commitJob.AddStep(step);


                }
                if (!NewEntry
                    && PostCommitSteps.Count > 0)
                {
                    foreach (var step in PostCommitSteps)
                    {
                        commitJob.AddStep(step);
                    }


                }
                commitJob.AddStep(commitStep);
                commitJob.AddStep(commitStep);
                if (NewEntry
                    && PostCommitSteps.Count > 0)
                {
                    foreach (var step in PostCommitSteps)
                    {
                        commitJob.AddStep(step);
                    }
                    commitJob.AddStep(commitStep);


                }
                var result = false;
                if (commitJob.Result != JobResult.Running && commitJob.Result != JobResult.Cancelled)
                {
                    result = commitJob.Run();

                }
                else
                {
                    result = true;
                }


                if (result)
                {
                    NewEntryProperties.Clear();
                    PostCommitSteps.Clear();
                    CommitSteps.Clear();
                    HasUnsavedChanges = false;

                    OnModelCommited?.Invoke();
                }

                return commitJob;
            }
            catch (DirectoryServicesCOMException ex)
            {
                throw new AppException(ex.Message + ex.ExtendedErrorMessage, ex);
            }

        }

        public virtual void Delete(bool forceDeleteChildren = false)
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
                        if (forceDeleteChildren && DirectoryEntry != null)
                        {
                            var children = DirectoryEntry.Children;
                            if (children != null)
                            {
                                foreach (DirectoryEntry child in children)
                                {
                                    DirectoryEntry.Children.Remove(child);
                                }
                            }

                        }
                        DirectoryEntry?.Parent.Children.Remove(DirectoryEntry);
                        IsDeleted = true;
                        OnModelDeleted?.Invoke();
                        break;
                    default:
                        throw new AppException("Deleting that object type is not supported yet.");


                }

            }
            catch (UnauthorizedAccessException ex)
            {
                throw new AppException("The application directory user does not " +
                    "have permission to delete entries", ex);
            }
            catch (DirectoryServicesCOMException ex)
            {
                switch (ex.HResult)
                {
                    case -2147016683:
                        throw new AppException("The object contains child objects.");
                }
                throw new AppException("The application directory user does not " +
                    "have permission to delete entries", ex);
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Error(ex, "Unexpected error deleting directory entry");
            }
        }


        public virtual void DiscardChanges()
        {

            DirectoryEntry = null;
            HasUnsavedChanges = false;
            NewEntryProperties = [];
            CommitSteps.Clear();

            PostCommitSteps.Clear();
            if (SearchResult != null)
                FetchDirectoryEntry();

            OnChangesDiscarded?.Invoke();

        }
        private void FetchDirectoryEntry()
        {
            if (SearchResult is null) throw new CriticalActiveDirectoryException(Directory, nameof(SearchResult));

            DirectoryEntry = SearchResult.GetDirectoryEntry();

        }

        public virtual T? GetCustomProperty<T>(string propertyName)
        {
            try
            {
                return GetAttribute<T>(propertyName);
            }
            catch
            {
                return default;
            }
        }

        public virtual DateTime? GetDateTimeAttribute(string propertyName)
        {
            try
            {

                var com = GetAttribute<object>(propertyName);
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

            Parallel.ForEach(dcs, dc =>
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
                        if (searchResult != null)
                        {
                            var value = searchResult.GetDirectoryEntry().Properties[propertyName].Value;
                            lock (list)
                            {
                                list.Add((T)value);
                            }
                        }
                    }
                }
                catch
                {
                    lock (list)
                    {
                        list.Add(default);
                    }
                }
            });

            return list;
        }


        protected virtual T? GetAttribute<T>(string propertyName)
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
        /// Retrieves the requested property value from a local cache or by polling Active Directory.
        /// </summary>
        /// <typeparam name="T">The value type of the requested attribute.</typeparam>
        /// <param name="propertyName">The name of the attribute to retrieve.</param>
        /// <returns>The attribute's value, or a default value if not found.</returns>
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
                    throw new InvalidCastException("Bad casting attempt for " + propertyName + " to type " + typeof(T).FullName, ex);
                }
                catch (Exception ex)
                {
                    Loggers.ActiveDirectoryLogger.Error(ex, "Unexpected error while getting property value for {@PropertyName}", propertyName);
                }



                return default;

            }
            if (DirectoryEntry == null)
            {
                if (SearchResult != null && SearchResult.Properties.Contains(propertyName))
                    return (T?)SearchResult.Properties[propertyName][0];
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
                throw new InvalidCastException("Bad casting attempt for " + propertyName + " to type " + typeof(T).FullName, ex);

            }
            catch
            {
                return default;

            }

            try
            {
                if (DirectoryEntry != null && DirectoryEntry.Properties.Contains(propertyName))
                    return (T?)DirectoryEntry.Properties[propertyName].Value;

            }
            catch (InvalidCastException ex)
            {
                throw new InvalidCastException("Bad casting attempt for " + propertyName + " to type " + typeof(T).FullName, ex);

            }

            catch
            {
                return default;
            }
            return default;

        }

        protected virtual string? GetStringAttribute(string propertyName)
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

        protected virtual List<string>? GetStringListAttribute(string propertyName)
        {
            try
            {
                List<string> values = [];
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
                        rawValue = [str];
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
        public virtual void SetCustomProperty(string propertyName, object? value) => SetAttribute(propertyName, value);
        /// <summary>
        /// Sets an attribute value. Note that this change is uncommited, <see cref="CommitChanges"/>
        /// must be called afterwards for the change to persist.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        protected virtual void SetAttribute(string propertyName, object? value)
        {
            if (IsDeleted) throw new AppException("Cannot set values for a deleted entry.");
            try
            {

                if (!NewEntry)
                {

                    if (value == null || value is string strValue && strValue == "")
                    {
                        var currentValue = GetStringAttribute(propertyName);
                        if ((currentValue == null && value == null) || currentValue?.Equals(value) == true)
                        {
                            return;
                        }
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
                    SetNewProperty(propertyName, value);
                }


            }
            catch (ArgumentOutOfRangeException)
            {
                // This exception can occur if the attribute is not available for this object class in AD.
            }
        }


        protected DateTime? SetFileTimeAttribute(string attribute, DateTime? value)
        {
            if (value == null)
                value = CommonHelpers.ADS_NULL_TIME;
            var dateTime = value.Value;
            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
            }
            SetAttribute(attribute, dateTime.ToUniversalTime().ToFileTime().ToString());
            return value;
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

                DirectoryEntry?.Properties[propertyName].Remove(value);

                HasUnsavedChanges = true;

            }
            catch (ArgumentOutOfRangeException)
            {
                // This exception can occur if the attribute is not available for this object class in AD.
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
                // This exception can occur if the attribute is not available for this object class in AD.
            }
        }

        public virtual void Dispose()
        {
            DirectoryEntry?.Dispose();
            SearchResult = null;

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