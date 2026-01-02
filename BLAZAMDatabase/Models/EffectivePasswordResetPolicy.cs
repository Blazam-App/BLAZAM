using BLAZAM.Database.Models.Permissions;

namespace BLAZAM.Database.Models
{
    public class EffectivePasswordResetPolicy
    {
        public bool CanResetPassword { get; private set; }
        public bool RequirePIN { get; private set; }
        public bool RequireEmail { get; private set; }
        public int MinimumPINLength { get; private set; }
        public bool RequireQA { get; private set; }

        // Constructor takes the list of ALL delegates assigned to the user
        public EffectivePasswordResetPolicy(IEnumerable<PermissionDelegate> delegates)
        {
            if (delegates == null || !delegates.Any())
            {
                CanResetPassword = false;
                return;
            }

            // 1. Permission is Additive (If any role allows it, they can do it)
            CanResetPassword = delegates.Any(d => d.AllowPasswordReset);

            // 2. Security Constraints are Restrictive (Safety First)

            // If ANY role requires a PIN, we enforce it.
            RequirePIN = delegates.Any(d => d.RequirePINOnPasswordReset);

            // If ANY role requires a PIN, we enforce it.
            RequireEmail = delegates.Any(d => d.RequireEmailOnPasswordReset);

            // If ANY role requires Q&A, we enforce it.
            RequireQA = delegates.Any(d => d.RequireQAOnPasswordReset);

            // We take the MAXIMUM length defined across all roles to ensure 
            // the highest security standard is met.
            MinimumPINLength = delegates.Max(d => d.MinimumPINLength);
        }
    }
}