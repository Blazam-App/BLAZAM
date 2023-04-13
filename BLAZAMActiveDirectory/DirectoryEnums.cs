namespace BLAZAM.ActiveDirectory
{
    public enum DirectoryConnectionStatus
    {
        OK, ConnectionDown, BadCredentials, ContainerNotFound,
        Unconfigured,
        UnreachableConfiguration,
        ServerDown,
        Connecting,
        BadConfiguration,
        EncryptionError,
    }
}
