using BLAZAM.Database.Models;
using BLAZAM.Session.Interfaces;

namespace BLAZAM.Session
{
    /// <summary>
    /// Represents a Multi-Factor Authentication (MFA) request, storing state needed during an MFA challenge process.
    /// </summary>
    public class MFARequest : IEquatable<MFARequest?>
    {

        public readonly MfaType mfaType;
        /// <summary>
        /// The MFA token associated with this request (e.g., a state token from Duo). This field is read-only.
        /// </summary>
        public readonly string mfaToken;

        /// <summary>
        /// The URL to redirect the user to after successful MFA completion.
        /// </summary>
        public string redirectUrl;

        /// <summary>
        /// The application user state associated with this MFA attempt.
        /// </summary>
        public IApplicationUserState user;

        /// <summary>
        /// Initializes a new instance of the <see cref="MFARequest"/> class.
        /// </summary>
        /// <param name="mfaToken">The MFA token.</param>
        /// <param name="redirectUrl">The URL to redirect to after MFA.</param>
        /// <param name="user">The user state initiating the MFA request.</param>
        public MFARequest(MfaType mfaType, string mfaToken, string redirectUrl, IApplicationUserState user)
        {
            this.mfaType = mfaType;
            this.mfaToken = mfaToken;
            this.redirectUrl = redirectUrl;
            this.user = user;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="MFARequest"/> object. Equality is based solely on the <see cref="mfaToken"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is an <see cref="MFARequest"/> and its <see cref="mfaToken"/> matches the current object's token; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is MFARequest other)
            {
                return other.mfaToken.Equals(mfaToken);
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified <see cref="MFARequest"/> object is equal to the current <see cref="MFARequest"/> object. Equality is based solely on the <see cref="mfaToken"/>.
        /// </summary>
        /// <param name="other">The <see cref="MFARequest"/> to compare with the current object.</param>
        /// <returns>True if the <see cref="mfaToken"/> of both objects matches; otherwise, false.</returns>
        public bool Equals(MFARequest? other)
        {
            return other is not null &&
                   mfaToken == other.mfaToken;
        }

        /// <summary>
        /// Serves as the default hash function. The hash code is based on the <see cref="mfaToken"/>.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(mfaToken);
        }

        /// <summary>
        /// Compares two <see cref="MFARequest"/> objects for equality. Equality is based solely on the <see cref="mfaToken"/>.
        /// </summary>
        /// <param name="left">The first <see cref="MFARequest"/> to compare.</param>
        /// <param name="right">The second <see cref="MFARequest"/> to compare.</param>
        /// <returns>True if the objects are considered equal; otherwise, false.</returns>
        public static bool operator ==(MFARequest? left, MFARequest? right)
        {
            return EqualityComparer<MFARequest>.Default.Equals(left, right);
        }

        /// <summary>
        /// Compares two <see cref="MFARequest"/> objects for inequality. Equality is based solely on the <see cref="mfaToken"/>.
        /// </summary>
        /// <param name="left">The first <see cref="MFARequest"/> to compare.</param>
        /// <param name="right">The second <see cref="MFARequest"/> to compare.</param>
        /// <returns>True if the objects are not considered equal; otherwise, false.</returns>
        public static bool operator !=(MFARequest? left, MFARequest? right)
        {
            return !(left == right);
        }
    }
}