using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Services
{
    internal static class DirectoryCache
    {
        private static ConcurrentDictionary<string, EntryCache> DirectoryEntries = new();
        
        public static EntryCache? GetEntryCache(string dn)
        {
            if (DirectoryEntries.ContainsKey(dn) && DateTime.Now - DirectoryEntries[dn].LastUpdated < TimeSpan.FromMinutes(2))
            {
                DirectoryEntries[dn].LastUpdated = DateTime.Now;
                return DirectoryEntries[dn];
            }
            return null;
        }
        public static void SetEntryCache(string dn, Dictionary<string, object?> attributes)
        {
            DirectoryEntries[dn] = new EntryCache(attributes);
        }
        public static void Clear(string dn)
        {
            if (DirectoryEntries.ContainsKey(dn))
            {
                DirectoryEntries.Remove(dn,out var test);
            }
        }
    }
    internal class EntryCache
    {
        internal DateTime LastUpdated { get; set; }
        internal Dictionary<string, object?> Attributes { get; set; }

        public EntryCache(Dictionary<string, object?> attributes)
        {
            Attributes = attributes;
            LastUpdated = DateTime.Now;
        }
    }
}
