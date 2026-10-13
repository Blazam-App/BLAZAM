using BLAZAM.Logger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Services
{
    public static class DirectoryCache
    {
        private static bool _purgerRunning = false;
        private static Timer? _purgerTimer = null;
        public const int CacheExpirationMinutes = 10;
        private static ConcurrentDictionary<string, EntryCache> DirectoryEntries = new();

        public static EntryCache? GetEntryCache(string dn)
        {
            if (!_purgerRunning)
            {
                _purgerTimer = new Timer(PurgeExpiredEntries,null,TimeSpan.FromMinutes());
            }
            if (DirectoryEntries.ContainsKey(dn)){
                if (DateTime.Now > DirectoryEntries[dn].ExpiresAt)
                {
                    Clear(dn);
                    return null;
                }
                return DirectoryEntries[dn];
            }
            Loggers.ActiveDirectoryLogger.Debug("DirectoryCache: Entry not found in cache for DN:{DN} ",dn);
            return null;
        }

        private static void PurgeExpiredEntries(object? state)
        {
            throw new NotImplementedException();
        }

        public static void SetEntryCache(string dn, Dictionary<string, object?> attributes)
        {
            DirectoryEntries[dn] = new EntryCache(attributes);
        }
        public static void ClearAll()
        {
            DirectoryEntries.Clear();

        }
        public static void Clear(string? dn)
        {
            if (dn == null) return;

            if (DirectoryEntries.ContainsKey(dn))
            {
                DirectoryEntries.Remove(dn, out var test);
            }
        }
    }
    public class EntryCache
    {
        internal DateTime Created { get; set; }
        internal DateTime ExpiresAt { get; set; }
        internal Dictionary<string, object?> Attributes { get; set; }

        public EntryCache(Dictionary<string, object?> attributes)
        {
            Attributes = attributes;
            ExpiresAt = DateTime.Now.AddMinutes(DirectoryCache.CacheExpirationMinutes);
            Created = DateTime.Now;
        }
    }
}
