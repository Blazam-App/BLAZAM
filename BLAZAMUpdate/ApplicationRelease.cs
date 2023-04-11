using Octokit;

namespace BLAZAM.Update
{
    public class ApplicationRelease : IApplicationRelease
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