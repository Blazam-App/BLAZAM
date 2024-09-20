using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BLAZAM.Server
{
    public static class ProgramEvents
    {
        /// <summary>
        /// Called when permission are changed by an admin
        /// </summary>
        public static AppEvent PermissionsChanged { get; set; }

        /// <summary>
        /// Send event so each user can update permissions
        /// </summary>
        public static void InvokePermissionsChanged()
        {
            PermissionsChanged?.Invoke();
        }

        /// <summary>
        /// Called when a template is added or removed
        /// </summary>
        public static AppEvent TemplatesChanged { get; set; }

        /// <summary>
        /// Send event so each user can update the templates in real time
        /// </summary>
        public static void InvokeTemplatesChanged()
        {
            TemplatesChanged?.Invoke();
        }



    }
}
