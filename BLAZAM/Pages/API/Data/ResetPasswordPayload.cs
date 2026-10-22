namespace BLAZAM.Pages.API.Data
{
    /// <summary>
    /// Request payload for resetting a user's password.
    /// </summary>
    public class ResetPasswordPayload
    {
        /// <summary>
        /// The new password to set for the user account.
        /// </summary>
        public string NewPassword { get; set; }

        /// <summary>
        /// Indicates whether the user must change their password at next logon.
        /// </summary>
        public bool RequireChangeAtNextLogon { get; set; } = true;
    }
}
