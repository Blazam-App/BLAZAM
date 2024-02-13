
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BLAZAM.Database.Context
{
    public interface IEFCoreDbContext : IDisposable
    {

        IEnumerable<string> AppliedMigrations { get; }

        ChangeTracker ChangeTracker { get; }
        DatabaseFacade Database { get; }
        IEnumerable<string> PendingMigrations { get; }

        bool EntityIsTracked<TEntry>(TEntry entry);
        bool IsSeeded();
        int SaveChanges();
        int SaveChanges(bool acceptAllChangesOnSuccess);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);
    }
}