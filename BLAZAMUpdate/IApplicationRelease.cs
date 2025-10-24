namespace BLAZAM.Update
{
    public interface IApplicationRelease
    {
        string? DownloadURL { get; }
        long? ExpectedSize { get; }
        string Branch { get; set; }
        ApplicationVersion Version { get; set; }
        string? ReleaseNotes { get; }
        bool? PreviewRelease { get; }
        DateTimeOffset? ReleaseTime { get; }
    }
}