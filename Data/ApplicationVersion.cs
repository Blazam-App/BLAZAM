using System.Diagnostics;
using System.Reflection;

namespace BLAZAM.Server.Data
{
    public class ApplicationVersion : IComparable
    {
        public Version AssemblyVersion { get; private set; }
        public string BuildNumber { get; private set; }
        public string Version { get => AssemblyVersion.ToString() + "." + BuildNumber; }
        public string ShortVersion { get => AssemblyVersion.ToString(); }
        /// <summary>
        /// Reads the running assembly and constructs a version object of the
        /// current application version.
        /// </summary>
        /// <param name="shortVersion"></param>
        public ApplicationVersion()
        {
            string[] assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString().Split(".");
           
            AssemblyVersion = new Version(int.Parse(assemblyVersion[0]), int.Parse(assemblyVersion[1]), int.Parse(assemblyVersion[2]));

            BuildNumber = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
        }
        public ApplicationVersion(Version assemblyVersion, string buildNumber)
        {

            AssemblyVersion = assemblyVersion;
            BuildNumber = buildNumber;
        }

        public ApplicationVersion(string fullVersionString)
        {
            string[] versionFragments = fullVersionString.Split('.');
            AssemblyVersion = new Version(versionFragments[0] + "." + versionFragments[1] + "." + versionFragments[2]);
            string buildNumber = "";
            for (int x = 3; x < versionFragments.Length; x++)
            {
                buildNumber += versionFragments[x];
                if (x + 1 != versionFragments.Length)
                    buildNumber += ".";
            }
            BuildNumber = buildNumber;
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
            if(obj is ApplicationVersion other)
            {
                if (AssemblyVersion.CompareTo(other.AssemblyVersion) < 0)
                {
                    return 1;
                }
                else
                    return BuildNumber.CompareTo(other.BuildNumber);
            }
            return -1;
        }
    }
}