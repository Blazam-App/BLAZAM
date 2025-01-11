using BLAZAM.Session.Interfaces;

namespace BLAZAM.Server.Data.Services
{
    public class MFARequest : IEquatable<MFARequest?>
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

        public bool Equals(MFARequest? other)
        {
            return other is not null &&
                   mfaToken == other.mfaToken;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(mfaToken);
        }

        public static bool operator ==(MFARequest? left, MFARequest? right)
        {
            return EqualityComparer<MFARequest>.Default.Equals(left, right);
        }

        public static bool operator !=(MFARequest? left, MFARequest? right)
        {
            return !(left == right);
        }
    }
}