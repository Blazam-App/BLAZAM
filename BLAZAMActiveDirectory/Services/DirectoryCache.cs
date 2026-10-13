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
        public const int CacheExpirationMinutes = 1;
        private static ConcurrentDictionary<string, EntryCache> DirectoryEntries = new();
        public static int Count => DirectoryEntries.Count;

        public static EntryCache? GetEntryCache(string dn)
        {
            if (!_purgerRunning)
            {
                _purgerRunning = true;
                _purgerTimer = new Timer(PurgeExpiredEntries, null, TimeSpan.FromMinutes(CacheExpirationMinutes), TimeSpan.FromMinutes(DirectoryCache.CacheExpirationMinutes));
            }
            DirectoryEntries.TryGetValue(dn, out var entry);
            if (entry!=null)
            {
                if (DateTime.Now > entry.ExpiresAt)
                {
                    Clear(dn);
                    return null;
                }
                return entry;
            }
            Loggers.ActiveDirectoryLogger.Debug("DirectoryCache: Entry not found in cache for DN:{DN} ", dn);
            return null;
        }

        private static void PurgeExpiredEntries(object? state)
        {
            try
            {
                var cacheCopy = DirectoryEntries.ToList();
                var now = DateTime.Now;
                foreach (var entry in cacheCopy.Where(c => now > c.Value.ExpiresAt))
                {
                    try
                    {
                        Clear(entry.Key);
                    }
                    catch (Exception ex)
                    {
                        Loggers.ActiveDirectoryLogger.Error(ex, "Error purging expired entry from DirectoryCache for DN:{DN}", entry.Key);
                    }
                }
            }
            catch (Exception ex)
            {
                Loggers.ActiveDirectoryLogger.Error(ex, "Error purging expired entries from DirectoryCache");
            }
        }

        public static void SetEntryCache(string dn, Dictionary<string, object?> attributes, int lifetimeInMinutes = CacheExpirationMinutes)
        {
            DirectoryEntries[dn] = new EntryCache(attributes, lifetimeInMinutes);
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

        public EntryCache(Dictionary<string, object?> attributes, int lifetimeInMinutes = DirectoryCache.CacheExpirationMinutes)
        {
            Attributes = attributes;
            ExpiresAt = DateTime.Now.AddMinutes(lifetimeInMinutes);
            Created = DateTime.Now;
        }
    }
}
