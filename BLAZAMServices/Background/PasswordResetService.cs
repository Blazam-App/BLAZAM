using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Database.Models.User;

namespace BLAZAM.Services.Background
{
    public class PasswordResetService
    {
        private readonly static List<AuthorizedPasswordResetRequest> _authorizedRequests = new List<AuthorizedPasswordResetRequest>();
        private readonly object _authorizedRequestsLock = new();
        public bool IsRequestAuthorized(IADUser user, string token)
        {
            lock (_authorizedRequestsLock)
            {
                var request = _authorizedRequests.FirstOrDefault(r => r.User.SAMAccountName == user.SAMAccountName && r.Token == token);
                if (request != null && request.Expiration > DateTime.UtcNow)
                {
                    _authorizedRequests.Remove(request);
                    return true;
                }
                return false;
            }
        }
        public bool IsRequestAuthorized(string mfaToken, string token)
        {
            lock (_authorizedRequestsLock)
            {
                var request = _authorizedRequests.FirstOrDefault(r => r.MFAToken == mfaToken && r.Token == token);
                if (request != null && request.Expiration > DateTime.UtcNow)
                {
                    return true;
                }
                return false;
            }
        }
        public IADUser? GetRequestUser(string mfaToken, string token)
        {
            lock (_authorizedRequestsLock)
            {
                var request = _authorizedRequests.FirstOrDefault(r => r.MFAToken == mfaToken && r.Token == token);
                if (request != null && request.Expiration > DateTime.UtcNow)
                {
                    return request.User;
                }
                return null;
            }
        }
        public void AddAuthorizedRequest(IADUser user, string? mFAToken)
        {
            lock (_authorizedRequestsLock)
            {
                _authorizedRequests.Add(new(user, mFAToken));
            }

        }
        public AuthorizedPasswordResetRequest? GetPasswordResetRequestFromMFAToken(string state)
        {
            lock (_authorizedRequestsLock)
            {
                var matching = _authorizedRequests.FirstOrDefault(x => x.MFAToken.Equals(state));
                return matching;
            }
        }
    }
    public class AuthorizedPasswordResetRequest
    {
        public IADUser User { get; set; }
        public DateTime Expiration { get; set; }

        public string? MFAToken { get; set; }

        public string Token { get; set; } = Guid.NewGuid().ToString();
        public AuthorizedPasswordResetRequest(IADUser user, string? mFAToken)
        {
            User = user;
            Expiration = DateTime.UtcNow.AddMinutes(1);
            MFAToken = mFAToken;
        }
    }
}
