using System.Diagnostics;
using System.Reflection;

namespace BLAZAM.Common.Data
{
    public class ApplicationVersion : IComparable
    {
        public Version AssemblyVersion { get; private set; }
        public string? BuildNumber { get; private set; }
        public string Version { get => AssemblyVersion.ToString() + "." + BuildNumber; }
        public string ShortVersion { get => AssemblyVersion.ToString(); }
        /// <summary>
        /// Reads the running assembly and constructs a version object of the
        /// current application version.
        /// </summary>
        /// <param name="shortVersion"></param>
        public ApplicationVersion(Assembly executingAssembly)
        {
            string[]? assemblyVersion = (executingAssembly.GetName().Version?.ToString().Split(".")) ?? throw new ApplicationException("The assembly version of the running app could not be read.");
            AssemblyVersion = new Version(int.Parse(assemblyVersion[0]), int.Parse(assemblyVersion[1]), int.Parse(assemblyVersion[2]));

            BuildNumber = FileVersionInfo.GetVersionInfo(executingAssembly.Location).ProductVersion;
        }
        public ApplicationVersion(Version assemblyVersion, string? buildNumber)
        {

            AssemblyVersion = assemblyVersion;
            BuildNumber = buildNumber;
        }

        public ApplicationVersion(string fullVersionString)
        {
            string[] versionFragments = fullVersionString.Split('.');
            AssemblyVersion = new Version(versionFragments[0] + "." + versionFragments[1] + "." + versionFragments[2]);
            if (versionFragments.Length > 3)
            {
                string buildNumber = "";
                for (int x = 3; x < versionFragments.Length; x++)
                {
                    buildNumber += versionFragments[x];
                    if (x + 1 != versionFragments.Length)
                        buildNumber += ".";
                }
                BuildNumber = buildNumber;
            }
        }

        public override int GetHashCode()
        {
            return Version.GetHashCode();
        }

        public override string? ToString()
        {
            return Version;
        }

        public int CompareTo(object? obj)
        {
            if (obj is ApplicationVersion other)
            {
                if (AssemblyVersion.CompareTo(other.AssemblyVersion) != 0)
                {
                    return AssemblyVersion.CompareTo(other.AssemblyVersion);
                }
                else
                {
                    if (BuildNumber != null && other.BuildNumber != null)
                        return BuildNumber.CompareTo(other.BuildNumber);
                    else
                        return 0;
                }
            }
            return 1;
        }
    }
}