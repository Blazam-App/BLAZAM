using Octokit;

namespace BLAZAM.Server.Data.Services.Update
{
    internal class ApplicationReleaseBranches
    {
        public const string Stable = "Stable"; 
        public const string Nightly = "Nightly"; 
        public const string Dev = "Dev"; 
    }
    internal class ApplicationRelease : IApplicationRelease
    {
        public string? DownloadURL
        {
            get
            {
                 return ReleaseAsset?.BrowserDownloadUrl;

            }
        }

        public long? ExpectedSize
        {
            get
            {
                  return ReleaseAsset?.Size;
            }
        }

        public string Branch { get; set; }
        public string? ReleaseNotes => GitHubRelease.Body;

        public bool? PreviewRelease => GitHubRelease.Prerelease;

        public DateTimeOffset? ReleaseTime => GitHubRelease.PublishedAt;

        public ApplicationVersion Version { get; set; }
        public Release? GitHubRelease { get; internal set; }
        private ReleaseAsset? ReleaseAsset => GitHubRelease?.Assets.FirstOrDefault();

    }
}