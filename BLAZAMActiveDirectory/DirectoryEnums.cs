namespace BLAZAM.ActiveDirectory
{
    /// <summary>
    /// The various states an <see cref="IActiveDirectory"/> can be in
    /// </summary>
    public enum DirectoryConnectionStatus
    {
        OK,
        ConnectionDown,
        BadCredentials,
        ContainerNotFound,
        Unconfigured,
        UnreachableConfiguration,
        ServerDown,
        Connecting,
        BadConfiguration,
        EncryptionError,
    }
}
