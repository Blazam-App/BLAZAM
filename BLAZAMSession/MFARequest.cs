using BLAZAM.Session.Interfaces;

namespace BLAZAM.Server.Data.Services
{
    public class MFARequest
    {
        public string mfaToken;
        public string redirectUrl;
        public IApplicationUserState user;

        public MFARequest(string mfaToken, string redirectUrl, IApplicationUserState user)
        {
            this.mfaToken = mfaToken;
            this.redirectUrl = redirectUrl;
            this.user = user;


        }

        public override bool Equals(object? obj)
        {
            if (obj is MFARequest other)
            {
                return other.mfaToken.Equals(mfaToken);
            }
            return false;
        }
    }
}