using BLAZAM.Common.Data;
using Microsoft.AspNetCore.Components.Authorization;

namespace BLAZAM.Common.Data
{
    public enum LoginResultStatus { OK,BadCredentials,UnauthorizedImpersonation, NoData,NoUsername, NoPassword, UnknownFailure,
        DeniedLogin
    }
    public class LoginResult
    {



        public LoginResult UnauthorizedImpersonation()
        {
            Status = LoginResultStatus.UnauthorizedImpersonation;

            return this;
        }


        public LoginResult BadCredentials()
        {
            Status = LoginResultStatus.BadCredentials;

            return this;
        }
          public LoginResult NoData()
        {
            Status = LoginResultStatus.NoData;

            return this;
        }
          public LoginResult NoUsername()
        {
            Status = LoginResultStatus.NoUsername;

            return this;
        }
          public LoginResult NoPassword()
        {
            Status = LoginResultStatus.NoPassword;

            return this;
        }



        public LoginResult UnknownFailure()
        {
            Status = LoginResultStatus.UnknownFailure;

            return this;
        }

        public AuthenticationState AuthenticationState { get; set; }
        public LoginResult Success(AuthenticationState result)
        {
            Status = LoginResultStatus.OK;
            AuthenticationState = result;
            return this;
        }

        public LoginResult DeniedLogin()
        {
            Status = LoginResultStatus.DeniedLogin;

            return this;
        }

        public LoginResultStatus Status
        {
            get; internal set;
        }
    }
}