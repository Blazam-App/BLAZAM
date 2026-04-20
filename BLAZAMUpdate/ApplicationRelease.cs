using Octokit;

namespace BLAZAM.Update
{
    public class ApplicationRelease : IApplicationRelease
    {
        /// <summary>
        /// Gets the direct download URL for the associated release asset, if available.
        /// </summary>
        public string? DownloadURL=> ReleaseAsset?.BrowserDownloadUrl;
        /// <summary>
        /// Gets the URL of the associated release, if available.
        /// </summary>
        public string? ReleaseURL=> GitHubRelease?.HtmlUrl;


        public long? ExpectedSize => ReleaseAsset?.Size;
            
        public string Branch { get; set; }
        public string? ReleaseNotes => GitHubRelease?.Body;

        public bool? PreviewRelease => GitHubRelease?.Prerelease;

        public DateTimeOffset? ReleaseTime => GitHubRelease?.PublishedAt;

        public ApplicationVersion Version { get; set; }
        public Release? GitHubRelease { get; internal set; }
        private ReleaseAsset? ReleaseAsset => GitHubRelease?.Assets[0];

    }
}