using BLAZAM.ActiveDirectory.Interfaces;
using BLAZAM.Common.Data;

namespace BLAZAM.ActiveDirectory
{
    /// <summary>
    /// An <see cref="IActiveDirectoryContext"/> intended for each web user connection
    /// </summary>
    public class ScopedActiveDirectoryContext : IDisposable
    {
        private IActiveDirectoryContextFactory _contextFactory;
        private bool disposedValue;

        public IActiveDirectoryContext Context { get; }

        /// <summary>
        /// Creates a new <see cref="IActiveDirectoryContext"/> scoped to the current Blazor session
        /// </summary>
        /// <param name="contextFactory">The system based context factory</param>
        public ScopedActiveDirectoryContext(IActiveDirectoryContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
            Context = _contextFactory.CreateActiveDirectoryContext();
            ApplicationStatistics.AddADContext();

        }
        /// <summary>
        /// Removes all created <see cref="IActiveDirectoryContext"/>'s created by this user's factory
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ApplicationStatistics.RemoveADContext();

                    Context.Dispose();

                }

                disposedValue = true;
            }
        }


        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
