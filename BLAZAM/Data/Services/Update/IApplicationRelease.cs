namespace BLAZAM.Server.Data.Services.Update
{
    internal interface IApplicationRelease
    {
        public string? DownloadURL { get; }
        public long? ExpectedSize { get; }
        public string Branch { get; set; }
        public ApplicationVersion Version { get; set; }
    }
}