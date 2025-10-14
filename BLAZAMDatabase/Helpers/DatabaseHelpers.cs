using System.Reflection;
using BLAZAM.Common.Data;
using BLAZAM.Database.Context;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Chat;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.Rules;
using BLAZAM.Database.Models.Templates;
using BLAZAM.Database.Models.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BLAZAM.Helpers
{
    public static class DatabaseHelpers
    {
        public static long GetMembersHash(this IEnumerable<AppUser> members)
        {
            long hash = 0;
            foreach (var member in members)
            {
                if (member.Username != null)
                {
                    hash += member.Username.GetAppHashCode();
                }
            }
            return hash;
        }
        public static List<NotificationType> GetNotificationTypes(this ActiveDirectoryObjectType objectType)
        {
            List<NotificationType> _triggerTypes = new();
            foreach (NotificationType type in Enum.GetValues(typeof(NotificationType)))
            {
                _triggerTypes.Add(type);
            }
            _triggerTypes = _triggerTypes.OrderBy(t => t.ToString()).ToList();
            return _triggerTypes.Where(t => t.IsNotificationAppropriateForObject(objectType)).ToList();

        }
        public static bool IsNotificationAppropriateForObject(this NotificationType notificationType, ActiveDirectoryObjectType type)
        {

            switch (type)
            {
                case ActiveDirectoryObjectType.User:
                case ActiveDirectoryObjectType.Computer:
                    switch (notificationType)
                    {
                        case NotificationType.Modify:
                        case NotificationType.Delete:
                        case NotificationType.Create:
                        case NotificationType.LockedOut:
                        case NotificationType.PasswordChange:
                        case NotificationType.Scheduled:
                        case NotificationType.Assign:
                        case NotificationType.Unassign:
                            return true;
                        default:
                            return false;
                    }
                case ActiveDirectoryObjectType.Group:
                case ActiveDirectoryObjectType.Contact:
                    switch (notificationType)
                    {
                        case NotificationType.Delete:
                        case NotificationType.Create:
                        case NotificationType.Unassign:
                        case NotificationType.Assign:
                        case NotificationType.Modify:
                        case NotificationType.Scheduled:
                            return true;
                        default:
                            return false;
                    }
                case ActiveDirectoryObjectType.Printer:
                case ActiveDirectoryObjectType.OU:
                    switch (notificationType)
                    {
                        case NotificationType.Delete:
                        case NotificationType.Create:
                        case NotificationType.Modify:
                        case NotificationType.Scheduled:
                            return true;
                        default:
                            return false;
                    }
                case ActiveDirectoryObjectType.BitLocker:
                    switch (notificationType)
                    {
                        case NotificationType.Delete:
                        case NotificationType.Scheduled:
                            return true;
                        default:
                            return false;
                    }
                case ActiveDirectoryObjectType.All:
                    return true;
                default:
                    return false;
            }
        }
        public static List<ActiveDirectoryObjectAction> ToList(this ActiveDirectoryObjectAction _enum)
        {
            return Enum.GetValues(typeof(ActiveDirectoryObjectAction)).Cast<ActiveDirectoryObjectAction>().ToList();
        }
        public static bool IsAdminOrDemo(this AppUser state)
        {
            return state.Username?.Equals("admin", StringComparison.InvariantCultureIgnoreCase) == true || state.Username?.Equals("demo", StringComparison.InvariantCultureIgnoreCase) == true;
        }
        public static List<TProperty> GetStaticProperties<TProperty>(this Type staticCollectionType)
        {

            // 2. Specify BindingFlags to get PUBLIC and STATIC members
            BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
            // DeclaredOnly prevents getting members from base classes (like object), though less relevant for static classes.

            // 3. Get all public static fields declared in this type
            FieldInfo[] fields = staticCollectionType.GetFields(flags);

            // 4. Filter fields to get only those of type ActiveDirectoryField
            //    and select their values.
            return fields
                .Where(fi => fi.FieldType == typeof(TProperty)) // Ensure the field is the correct type
                .Select(fi => (TProperty?)fi.GetValue(null)) // Get the static value (pass null for static fields)
                .Where(value => value != null) // Ensure the value isn't null
                .ToList();
        }

        public static void ConfigureModel(ModelBuilder modelBuilder, DatabaseContextBase context)
        {
            AddActiveDirectoryFieldData(modelBuilder);
            AddCustomActiveDirectoryFieldConfig(modelBuilder);
            AddAccessLevelConfig(modelBuilder);
            AddFieldAccessLevelData(modelBuilder);
            AddFieldAccessMappingConfig(modelBuilder);
            AddObjectAccessLevelData(modelBuilder);
            AddObjectAccessMappingConfig(modelBuilder);
            AddDirectoryTemplateConfig(modelBuilder);
            AddAutomationRuleConfig(modelBuilder);
            AddAutomationRuleActionConfig(modelBuilder);
            AddAutomationRuleOrFilterConfig(modelBuilder);
            AddAutomationRuleAndFilterConfig(modelBuilder);
            AddAutomationRuleActionFieldValueConfig(modelBuilder);
            AddDirectoryTemplateFieldValueConfig(modelBuilder);
            AddObjectActionData(modelBuilder);
            AddActionAccessMappingConfig(modelBuilder);
            AddPermissionMappingConfig(modelBuilder);
            AddAppSettingsConfig(modelBuilder, context);
            AddADSettingsConfig(modelBuilder, context);
            AddAuthenticationSettingsConfig(modelBuilder, context);
            AddEmailSettingsConfig(modelBuilder, context);
            AddAppUserConfig(modelBuilder);
            AddUserNotificationConfig(modelBuilder);
            AddPermissionDelegateConfig(modelBuilder);
            AddChatRoomConfig(modelBuilder);
            AddChatMessageConfig(modelBuilder);
            AddNotificationSubscriptionConfig(modelBuilder);
            AddUnreadChatMessageConfig(modelBuilder);

        }

        internal static void AddActiveDirectoryFieldData(ModelBuilder modelBuilder)
        {
            List<ActiveDirectoryField> activeDirectoryFields = typeof(ActiveDirectoryFields).GetStaticProperties<ActiveDirectoryField>();
            modelBuilder.Entity<ActiveDirectoryField>().HasData(activeDirectoryFields);
        }

        internal static void AddCustomActiveDirectoryFieldConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomActiveDirectoryField>().HasMany(x => x.ObjectTypes);
            modelBuilder.Entity<CustomActiveDirectoryField>().Navigation(x => x.ObjectTypes).AutoInclude();
        }

        internal static void AddAccessLevelConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccessLevel>(entity =>
            {
                entity.HasData(new AccessLevel { Id = 1, Name = "Deny All" });
                entity.Navigation(e => e.ObjectMap).AutoInclude();
                entity.Navigation(e => e.FieldMap).AutoInclude();
                entity.Navigation(e => e.ActionMap).AutoInclude();
            });
        }

        internal static void AddFieldAccessLevelData(ModelBuilder modelBuilder)
        {
            List<FieldAccessLevel> fieldAccessLevels = typeof(FieldAccessLevels).GetStaticProperties<FieldAccessLevel>();
            modelBuilder.Entity<FieldAccessLevel>().HasData(fieldAccessLevels);
        }

        internal static void AddFieldAccessMappingConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FieldAccessMapping>(entity =>
            {
                entity.Navigation(e => e.CustomField).AutoInclude();
                entity.Navigation(e => e.Field).AutoInclude();
                entity.Navigation(e => e.FieldAccessLevel).AutoInclude();
            });
        }

        internal static void AddObjectAccessLevelData(ModelBuilder modelBuilder)
        {
            List<ObjectAccessLevel> objectAccessLevels = typeof(ObjectAccessLevels).GetStaticProperties<ObjectAccessLevel>();
            modelBuilder.Entity<ObjectAccessLevel>(entity =>
            {
                entity.HasData(objectAccessLevels);
            });
        }

        internal static void AddObjectAccessMappingConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ObjectAccessMapping>(entity =>
            {
                entity.Navigation(e => e.ObjectAccessLevel).AutoInclude();
            });
        }

        internal static void AddDirectoryTemplateConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DirectoryTemplate>(entity =>
            {
                entity.Navigation(e => e.FieldValues).AutoInclude();
                entity.Navigation(e => e.AssignedGroupSids).AutoInclude();
            });
        }

        internal static void AddAutomationRuleConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AutomationRule>(entity =>
            {
                entity.Navigation(e => e.Actions).AutoInclude();
                entity.Navigation(e => e.Filters).AutoInclude();
            });
        }

        internal static void AddAutomationRuleActionConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AutomationRuleAction>(entity =>
            {
                entity.Navigation(e => e.GroupGuids).AutoInclude();
                entity.Navigation(e => e.FieldValues).AutoInclude();
            });
        }

        internal static void AddAutomationRuleOrFilterConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AutomationRuleOrFilter>(entity =>
            {
                entity.Navigation(e => e.AndFilters).AutoInclude();
            });
        }

        internal static void AddAutomationRuleAndFilterConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AutomationRuleAndFilter>(entity =>
            {
                entity.Navigation(e => e.CustomField).AutoInclude();
                entity.Navigation(e => e.Field).AutoInclude();
                entity.Property(e => e.TimeFrame)
                    .HasConversion(new ValueConverter<TimeSpan?, long?>(
                        v => v.HasValue ? v.Value.Ticks : (long?)null,
                        v => v.HasValue ? TimeSpan.FromTicks(v.Value) : (TimeSpan?)null
                    ));
            });
        }

        internal static void AddAutomationRuleActionFieldValueConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AutomationRuleActionFieldValue>(entity =>
            {
                entity.Navigation(e => e.Field).AutoInclude();
                entity.Navigation(e => e.CustomField).AutoInclude();
            });
        }

        internal static void AddDirectoryTemplateFieldValueConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DirectoryTemplateFieldValue>(entity =>
            {
                entity.Navigation(e => e.Field).AutoInclude();
                entity.Navigation(e => e.CustomField).AutoInclude();
            });
        }

        internal static void AddObjectActionData(ModelBuilder modelBuilder)
        {
            List<ObjectAction> objectActions = typeof(ObjectActions).GetStaticProperties<ObjectAction>();
            modelBuilder.Entity<ObjectAction>().HasData(objectActions);
        }

        internal static void AddActionAccessMappingConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActionAccessMapping>(entity =>
            {
                entity.Navigation(e => e.ObjectAction).AutoInclude();
            });
        }

        internal static void AddPermissionMappingConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PermissionMapping>(entity =>
            {
                entity.Navigation(e => e.AccessLevels).AutoInclude();
            });
        }

        internal static void AddAppSettingsConfig(ModelBuilder modelBuilder, DatabaseContextBase context)
        {
            modelBuilder.Entity<AppSettings>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
                if (context.Database.IsMySql())
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "Id = 1"));
                else
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "[Id] = 1"));
            });
        }

        internal static void AddADSettingsConfig(ModelBuilder modelBuilder, DatabaseContextBase context)
        {
            modelBuilder.Entity<ADSettings>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
                if (context.Database.IsMySql())
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "Id = 1"));
                else
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "[Id] = 1"));
            });
        }

        internal static void AddAuthenticationSettingsConfig(ModelBuilder modelBuilder, DatabaseContextBase context)
        {
            modelBuilder.Entity<AuthenticationSettings>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
                if (context.Database.IsMySql())
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "Id = 1"));
                else
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "[Id] = 1"));
                entity.HasData(new AuthenticationSettings
                {
                    Id = 1,
                    AdminPassword = "password"
                });
            });
        }

        internal static void AddEmailSettingsConfig(ModelBuilder modelBuilder, DatabaseContextBase context)
        {
            modelBuilder.Entity<EmailSettings>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
                if (context.Database.IsMySql())
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "Id = 1"));
                else
                    entity.ToTable(t => t.HasCheckConstraint("CK_Table_Column", "[Id] = 1"));
            });
        }

        internal static void AddAppUserConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasIndex(e => e.UserGUID).IsUnique();
                entity.Navigation(e => e.ReadNewsItems).AutoInclude();
                entity.Navigation(e => e.FavoriteEntries).AutoInclude();
                entity.Navigation(e => e.DashboardWidgets).AutoInclude();
            });
        }

        internal static void AddUserNotificationConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserNotification>(entity =>
            {
                entity.Navigation(e => e.Notification).AutoInclude();
            });
        }

        internal static void AddPermissionDelegateConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PermissionDelegate>(entity =>
            {
                entity.HasIndex(e => e.DelegateSid).IsUnique();
            });
        }

        internal static void AddChatRoomConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatRoom>(entity =>
            {
                entity.HasMany(e => e.Members).WithMany();
                entity.Navigation(e => e.Messages).AutoInclude();
                entity.Navigation(e => e.Members).AutoInclude();
            });
        }

        internal static void AddChatMessageConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.Navigation(e => e.User).AutoInclude();
            });
        }

        internal static void AddNotificationSubscriptionConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotificationSubscription>(entity =>
            {
                entity.Navigation(e => e.NotificationTypes).AutoInclude();
            });
        }

        internal static void AddUnreadChatMessageConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UnreadChatMessage>(entity =>
            {
                entity.Navigation(e => e.ChatMessage).AutoInclude();
            });
        }
    }
}
