using BLAZAM.Common.Data;
using BLAZAM.Database.Models;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.Templates;
using BLAZAM.Database.Models.User;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace BLAZAM.Helpers
{
    public static class DatabaseHelpers
    {


        /// <summary>
        /// Checks if the given field is in the template, whether editable or not
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static bool InTemplate(this DirectoryTemplate? template, IActiveDirectoryField field)
        {
            if (template == null)
            {
                return false;
            }
            return template.EffectiveFieldValues.Any(f => (f.Field != null && f.Field.FieldName == field.FieldName) || (f.CustomField != null && f.CustomField.FieldName == field.FieldName));

        }

        public static bool IsEditableField(this DirectoryTemplate template, IActiveDirectoryField field)
        {
            if (field is ActiveDirectoryField)
                return template.EffectiveFieldValues.Any(f => f.Field?.FieldName == field.FieldName && f.Editable);
            else
                return template.EffectiveFieldValues.Any(f => f.CustomField?.FieldName == field.FieldName && f.Editable);
        }

        public static bool IsRequiredField(this DirectoryTemplate template, IActiveDirectoryField field)
        {
            if (field is ActiveDirectoryField)
                return template.EffectiveFieldValues.Any(f => f.Field?.FieldName == field.FieldName && f.Required);
            else
                return template.EffectiveFieldValues.Any(f => f.CustomField?.FieldName == field.FieldName && f.Required);
        }
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
            List<NotificationType> _triggerTypes = Enum.GetValues(typeof(NotificationType)).Cast<NotificationType>().ToList();
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

        public static bool IsDemo(this AppUser state)
        {
            return state.Username?.Equals("demo", StringComparison.InvariantCultureIgnoreCase) == true;
        }

        public static List<TProperty?> GetStaticProperties<TProperty>(this Type staticCollectionType)
        {

            BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;


            FieldInfo[] fields = staticCollectionType.GetFields(flags);


            return fields
                .Where(fi => fi.FieldType == typeof(TProperty)) // Ensure the field is the correct type
                .Select(fi => (TProperty?)fi.GetValue(null)) // Get the static value (pass null for static fields)
                .Where(value => value != null) // Ensure the value isn't null
                .ToList();
        }
        public static async Task<DirectoryTemplate?> LoadTemplateWithParents(this DbSet<DirectoryTemplate> dbSet, int? templateId)
        {

            if (templateId == null)
            {
                return null;
            }
            var template = await dbSet.Include(t => t.ParentTemplate).FirstOrDefaultAsync(t => t.Id.Equals(templateId));
            if (template?.ParentTemplate != null)
            {
                var parentTemplate = await dbSet.Include(t => t.ParentTemplate).FirstOrDefaultAsync(t => t.Id.Equals(template.ParentTemplate.Id));
                if (parentTemplate != null && parentTemplate.ParentTemplate != null)
                {
                    parentTemplate.ParentTemplate = await dbSet.LoadTemplateWithParents(parentTemplate.ParentTemplate.Id);
                }
                template.ParentTemplate = parentTemplate;
            }
            return template;

        }
    }
}
