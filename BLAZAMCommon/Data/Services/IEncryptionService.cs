namespace BLAZAM.Server.Data.Services
{
    public interface IEncryptionService
    {
        T? DecryptObject<T>(string? cipherText);
        string EncryptObject(object obj);
    }
}