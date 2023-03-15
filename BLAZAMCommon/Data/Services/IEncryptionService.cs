using BLAZAM.Common.Data;

namespace BLAZAM.Server.Data.Services
{
    public interface IEncryptionService
    {
        ServiceConnectionState Status { get; }

        T? DecryptObject<T>(string? cipherText);
        string EncryptObject(object obj);
    }
}