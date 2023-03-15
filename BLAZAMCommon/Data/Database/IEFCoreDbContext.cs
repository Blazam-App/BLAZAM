using BLAZAM.Common.Models.Database;
using BLAZAM.Common.Models.Database.Audit;
using BLAZAM.Common.Models.Database.Permissions;
using BLAZAM.Common.Models.Database.Templates;
using BLAZAM.Common.Models.Database.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BLAZAM.Common.Data.Database
{
    public interface IEFCoreDbContext : IDisposable
    {
      
        IEnumerable<string> AppliedMigrations { get; }
       
        ChangeTracker ChangeTracker { get; }
        DatabaseFacade Database { get; }
        IEnumerable<string> PendingMigrations { get; }

        bool IsSeeded();
        int SaveChanges();
        int SaveChanges(bool acceptAllChangesOnSuccess);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);
    }
}