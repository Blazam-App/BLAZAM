namespace BLAZAM.Common.Interfaces
{
    public interface IEncryptionService
    {
        ServiceConnectionState Status { get; }
        byte[] Key { get; }

        T? DecryptObject<T>(string? cipherText);
        string EncryptObject(object obj);
    }
}