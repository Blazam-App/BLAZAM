using BLAZAM.Helpers;
using Google.Authenticator;

namespace BLAZAM.Services.Duo
{
    public class GoogleAuthenticatorService
    {
        private readonly TwoFactorAuthenticator _authenticator;

        public GoogleAuthenticatorService()
        {
            _authenticator = new TwoFactorAuthenticator();
        }

        public SetupCode GenerateSetupCode(string accountName, string accountSecretKey)
        {
            return _authenticator.GenerateSetupCode("Blazam", accountName, accountSecretKey, false);
        }

        public bool ValidateTwoFactorPIN(SecureString accountSecretKey, string twoFactorCodeFromClient)
        {
            return _authenticator.ValidateTwoFactorPIN(accountSecretKey.ToPlainText(), twoFactorCodeFromClient, TimeSpan.FromMinutes(3));
        }
    }
}
