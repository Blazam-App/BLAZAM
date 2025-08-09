using System.Reflection;

namespace BLAZAM.Global.Data
{
    /// <summary>
    /// A representation of a version of the app.
    /// 
    /// </summary>
    /// <remarks>
    /// [0.8.4].[2023.11.09.1134]
    /// <para>
    /// 
    /// [AssemblyVersion][Build Number]
    /// </para>
    /// </remarks>
    public class PluginVersion : ApplicationVersion
    {
        public PluginVersion(Assembly executingAssembly) : base(executingAssembly)
        {
        }

        public PluginVersion(string fullVersionString) : base(fullVersionString)
        {
        }


        public PluginVersion(Version assemblyVersion, string? buildNumber) : base(assemblyVersion, buildNumber)
        {
        }
    }

}