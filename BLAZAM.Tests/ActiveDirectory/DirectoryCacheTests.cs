using BLAZAM.ActiveDirectory.Services;

namespace BLAZAM.Tests.ActiveDirectory
{
    public class DirectoryCacheTests
    {
        private static string NewDn() => $"CN={Guid.NewGuid()},DC=test,DC=local";

        [Fact]
        public void GetEntryCache_MissingEntry_ReturnsNull()
        {
            Assert.Null(DirectoryCache.GetEntryCache(NewDn()));
        }

        [Fact]
        public void SetEntryCache_ThenGet_ReturnsEntry()
        {
            var dn = NewDn();
            DirectoryCache.SetEntryCache(dn, new Dictionary<string, object?> { ["cn"] = "test" });

            var entry = DirectoryCache.GetEntryCache(dn);

            Assert.NotNull(entry);
            DirectoryCache.Clear(dn);
        }

        [Fact]
        public void SetEntryCache_ExistingDn_OverwritesEntry()
        {
            var dn = NewDn();
            DirectoryCache.SetEntryCache(dn, new Dictionary<string, object?>());
            var first = DirectoryCache.GetEntryCache(dn);

            DirectoryCache.SetEntryCache(dn, new Dictionary<string, object?>());
            var second = DirectoryCache.GetEntryCache(dn);

            Assert.NotSame(first, second);
            DirectoryCache.Clear(dn);
        }

        [Fact]
        public void GetEntryCache_ExpiredEntry_ReturnsNullAndRemoves()
        {
            var dn = NewDn();
            DirectoryCache.SetEntryCache(dn, new Dictionary<string, object?>(), lifetimeInMinutes: -1);

            Assert.Null(DirectoryCache.GetEntryCache(dn));
            // Removed, so a second lookup also misses
            Assert.Null(DirectoryCache.GetEntryCache(dn));
        }

        [Fact]
        public void Clear_RemovesEntry()
        {
            var dn = NewDn();
            DirectoryCache.SetEntryCache(dn, new Dictionary<string, object?>());

            DirectoryCache.Clear(dn);

            Assert.Null(DirectoryCache.GetEntryCache(dn));
        }

        [Fact]
        public void Clear_NullOrUnknownDn_DoesNotThrow()
        {
            DirectoryCache.Clear(null);
            DirectoryCache.Clear(NewDn());
        }

        [Fact]
        public void Count_ReflectsAddedEntry()
        {
            var dn = NewDn();
            DirectoryCache.SetEntryCache(dn, new Dictionary<string, object?>());

            Assert.True(DirectoryCache.Count >= 1);
            DirectoryCache.Clear(dn);
        }

        [Fact]
        public void EntryCache_Constructor_SetsTimestamps()
        {
            var attrs = new Dictionary<string, object?> { ["cn"] = "x" };
            var before = DateTime.Now;

            var entry = new EntryCache(attrs, 5);

            Assert.True(entry.Created >= before);
            Assert.Equal(entry.Created.AddMinutes(5), entry.ExpiresAt, TimeSpan.FromSeconds(1));
            Assert.Same(attrs, entry.Attributes);
        }
    }
}