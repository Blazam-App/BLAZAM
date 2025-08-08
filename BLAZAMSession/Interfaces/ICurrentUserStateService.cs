namespace BLAZAM.Session.Interfaces
{
    /// <summary>
    /// Defines a contract for a scoped service that provides access to the <see cref="IApplicationUserState"/> for the current user in the request scope.
    /// </summary>
    public interface ICurrentUserStateService : IDisposable
    {
        /// <summary>
        /// Gets or sets the <see cref="IApplicationUserState"/> for the current request scope.
        /// </summary>
        IApplicationUserState State { get; set; }

        /// <summary>
        /// Gets the username of the current user, derived from the <see cref="State"/>.
        /// </summary>
        string Username { get; }
    }
}