using System.Reflection;
using BLAZAM.Common.Data;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.User;

namespace BLAZAM.Helpers
{
    public static class DatabaseHelpers
    {
        public static long GetMembersHash(this IEnumerable<AppUser> members)
        {
            long hash = 0;
            foreach (var member in members)
            {
                hash += member.Username.GetAppHashCode();
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

            //var Name = action.ToString();
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
    }
}
