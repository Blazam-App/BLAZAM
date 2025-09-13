using System.Reflection;
using BLAZAM.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BLAZAM.Database.Data
{
    /// <summary>
    /// Custom implementation to find migrations across main and plugin assemblies.
    /// </summary>
    public class CompositeMigrationsAssembly : IMigrationsAssembly
    {
        private readonly Assembly _mainAssembly;
        private readonly IReadOnlyList<Assembly> _pluginAssemblies;
        private readonly DbContext _context;
        private readonly Dictionary<string, TypeInfo> _migrationTypes;
        private readonly Dictionary<string, Migration> _migrations;

        public CompositeMigrationsAssembly(ICurrentDbContext currentContext, IDbContextOptions options)
        {
            _context = currentContext.Context;
            // Main assembly
            _mainAssembly = _context.GetType().Assembly;
            // Plugin assemblies: customize this as needed for your plugin system
            _pluginAssemblies = ApplicationInfo.loadedPlugins.Select(p => p.Assembly).ToList();

            // Find all migration types
            _migrationTypes = new Dictionary<string, TypeInfo>();
            foreach (var assembly in new[] { _mainAssembly }.Concat(_pluginAssemblies))
            {
                foreach (var type in assembly.DefinedTypes)
                {
                    if (typeof(Migration).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        // Only include migrations for the current DbContext type
                        var dbContextAttribute = type.GetCustomAttribute<DbContextAttribute>();
                        if (dbContextAttribute == null || dbContextAttribute.ContextType == _context.GetType())
                        {
                            // Get migration id from attribute or fallback to class name
                            var migrationAttribute = type.GetCustomAttribute<MigrationAttribute>();
                            var migrationId = migrationAttribute?.Id ?? type.Name;
                            _migrationTypes[migrationId] = type;
                        }
                    }
                }
            }

            // Instantiate migrations
            _migrations = _migrationTypes.ToDictionary(
                kvp => kvp.Key,
                kvp => (Migration)Activator.CreateInstance(kvp.Value.AsType())!
            );
        }

        public IReadOnlyDictionary<string, TypeInfo> Migrations => _migrationTypes;

        public ModelSnapshot? ModelSnapshot
        {
            get
            {
                // Find the model snapshot type
                var snapshotType = _mainAssembly.DefinedTypes
                    .FirstOrDefault(t => typeof(ModelSnapshot).IsAssignableFrom(t) && !t.IsAbstract);
                return snapshotType != null
                    ? (ModelSnapshot?)Activator.CreateInstance(snapshotType.AsType())
                    : null;
            }
        }

        public Assembly Assembly => _mainAssembly;

        public Migration CreateMigration(TypeInfo migrationClass, string activeProvider)
        {
            var migration = (Migration)Activator.CreateInstance(migrationClass.AsType())!;
            migration.ActiveProvider = activeProvider;
            return migration;
        }
        public string? FindMigrationId(string nameOrId)
        {
            // Try to find by exact key
            if (_migrationTypes.ContainsKey(nameOrId))
                return nameOrId;

            // Try to find by matching the start of the migration name (as EF does)
            var match = _migrationTypes.Keys.FirstOrDefault(k => k.StartsWith(nameOrId, StringComparison.OrdinalIgnoreCase));
            return match;
        }
    }
}
