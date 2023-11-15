using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace BLAZAM.Common.Data
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
        /// <summary>
        /// The UTC time of release for this version
        /// </summary>
        /// <remarks>
        /// This is calculated from the build number
        /// </remarks>
        public DateTime ReleaseDate
        {
            get
            {
                DateTime release = DateTime.MinValue;
                var buildNumberParts = BuildNumber.Split('.');
                string year="";
                string month="";
                string day="";
                string time="";
                for (int x = 0; x < buildNumberParts.Length; x++)
                {
                    switch (x)
                    {
                        case 0:
                            year = buildNumberParts[x];
                            break;
                        case 1:
                            month = buildNumberParts[x];

                            break;
                        case 2:
                            day = buildNumberParts[x];

                            break;
                        case 3:
                            time = buildNumberParts[x];
                            time=time.Insert(2, ":");
                            break;
                    }
                }
                DateTime.TryParse((month+"/"+day+"/"+year + " "+ time+" Z"), out release);
                return release;
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