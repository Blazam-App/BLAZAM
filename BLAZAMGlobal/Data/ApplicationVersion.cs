using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using BLAZAM.Global.Exceptions;

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
    public class ApplicationVersion : IComparable, IEquatable<ApplicationVersion?>
    {
        public Version AssemblyVersion { get; private set; }
        /// <summary>
        /// The forth and remaining . segments
        /// </summary>
        /// <remarks>
        /// For Blazam this is the build time
        /// </remarks>
        public string? BuildNumber { get; private set; }

        /// <summary>
        /// The full version number in string form
        /// </summary>
        public string Version { get => AssemblyVersion.ToString() + "." + BuildNumber; }



        /// <summary>
        /// Only the first three . segments of the version number
        /// </summary>
        /// <remarks>
        /// Same as the <see cref="AssemblyVersion"/>
        /// </remarks>
        public string ShortVersion { get => AssemblyVersion.ToString(); }
        /// <summary>
        /// Reads the running assembly and constructs a version object of the
        /// current application version.
        /// </summary>
        /// <param name="shortVersion"></param>
        public ApplicationVersion(Assembly executingAssembly)
        {
            string[]? assemblyVersion = (executingAssembly.GetName()?.Version?.ToString().Split(".")) ?? throw new AppException("The assembly version of the running app could not be read.");
            AssemblyVersion = new Version(int.Parse(assemblyVersion[0]), int.Parse(assemblyVersion[1]), int.Parse(assemblyVersion[2]));
            var fileInfo = FileVersionInfo.GetVersionInfo(executingAssembly.Location);
            var productVersion = fileInfo.ProductVersion;
            if (productVersion?.Contains("+") == true)
            {
                productVersion = productVersion.Split("+")[0];
            }
            BuildNumber = productVersion;
        }

        /// <summary>
        /// Builds a new <see cref="ApplicationVersion"/> from an existing <see cref="Version"/> and a build number string if provided
        /// </summary>
        /// <param name="assemblyVersion"></param>
        /// <param name="buildNumber"></param>
        public ApplicationVersion(Version assemblyVersion, string? buildNumber)
        {

            AssemblyVersion = assemblyVersion;
            BuildNumber = buildNumber;
        }

        /// <summary>
        /// Creates a new <see cref="ApplicationVersion"/> from a raw string of a version number
        /// </summary>
        /// <remarks>
        /// The version number must follow Major.Minor.Build standards
        /// </remarks>
        /// <param name="fullVersionString"></param>
        public ApplicationVersion(string fullVersionString)
        {
            string[] versionFragments = fullVersionString.Split('.');
            if (versionFragments.Length < 3)
            {
                throw new ArgumentException("Version should have at least three components");
            }
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
        public DateTime? ReleaseDate
        {
            get
            {
                DateTime? release = null;
                try
                {

                    var buildNumberParts = BuildNumber?.Split('.');
                    if (buildNumberParts == null || buildNumberParts.Length < 4)
                    {
                        return null; // Not enough parts to determine a date
                    }

                    string year = "";
                    string month = "";
                    string day = "";
                    string time = "";

                    // Assuming the build number is in the format "YYYY.MM.DD.HHMM"
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
                                time = time.Insert(2, ":");
                                break;
                        }
                    }
                    // Construct the date string and parse it
                    // 1. Combine your date and time parts into one string
                    string dateString = $"{month}/{day}/{year} {time} Z";

                    // 2. Define the exact format of your string
                    //    - M/d/yyyy handles single or double-digit months/days
                    //    - HH:mm:ss is for 24-hour time
                    //    - The 'Z' is treated as a literal character indicating UTC
                    string format = "M/d/yyyy HH:mm:ss 'Z'";

                    DateTime parsedDateTime;

                    // 3. Use TryParseExact with the specified format and provider
                    parsedDateTime = DateTime.Parse(dateString, CultureInfo.InvariantCulture);
                    release = parsedDateTime;

                }
                catch
                {
                    //return default
                }
                return release;
            }
        }



        public override int GetHashCode()
        {
            return Version.GetHashCode();
        }

        public override string ToString()
        {
            return Version;
        }

        public bool NewerThan(ApplicationVersion version)
        {
            return CompareTo(version) > 0;
        }

        public static bool operator !=(ApplicationVersion version1, ApplicationVersion version2)
        {
            return version1.CompareTo(version2) != 0;
        }
        public static bool operator ==(ApplicationVersion version1, ApplicationVersion version2)
        {
            return version1.CompareTo(version2) == 0;
        }
        public static bool operator >=(ApplicationVersion version1, ApplicationVersion version2)
        {
            return version1.CompareTo(version2) >= 0;
        }
        public static bool operator <=(ApplicationVersion version1, ApplicationVersion version2)
        {
            return version1.CompareTo(version2) <= 0;
        }
        public static bool operator <(ApplicationVersion version1, ApplicationVersion version2)
        {
            return version1.CompareTo(version2) < 0;
        }
        public static bool operator >(ApplicationVersion version1, ApplicationVersion version2)
        {
            return version1.CompareTo(version2) > 0;
        }
        public bool OlderThan(ApplicationVersion version)
        {
            return CompareTo(version) < 0;
        }


        public bool SameVersionAs(ApplicationVersion version)
        {
            return CompareTo(version) < 0;
        }
        /// <summary>
        /// <para>
        /// If return is greater than one this version is newer
        /// </para>
        /// <para>
        /// If return is less than one this version is older
        /// </para>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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

        public override bool Equals(object? obj)
        {
            return Equals(obj as ApplicationVersion);
        }

        public bool Equals(ApplicationVersion? other)
        {
            return other is not null &&
                   Version == other.Version;
        }
    }

}