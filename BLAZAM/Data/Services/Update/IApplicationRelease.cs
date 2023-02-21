namespace BLAZAM.Server.Data.Services.Update
{
    internal interface IApplicationRelease
    {
        public string DownloadURL { get; set; }
        public long ExpectedSize { get; set; }
        public string Branch { get; set; }
        public ApplicationVersion Version { get; set; }
    }
}