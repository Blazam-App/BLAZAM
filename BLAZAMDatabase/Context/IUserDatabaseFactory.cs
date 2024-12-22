using BLAZAM.Common.Data.Database;
using BLAZAM.Database.Exceptions;

namespace BLAZAM.Database.Context
{
    /// <summary>
    /// The primary database factory for BLAZAM.
    /// Creates <see cref="IDatabaseContext"/> types 
    /// that can have a number of database type backings
    /// </summary>
    public interface IUserDatabaseFactory : IAppDatabaseFactory
    {
    }
}