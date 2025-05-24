using AngleSharp.Dom;
using BLAZAM.ActiveDirectory.Adapters;
using BLAZAM.ActiveDirectory.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLAZAM.ActiveDirectory.Mocks // Or your preferred testing namespace
{
    public class MockDirectoryEntry : IDirectoryEntry
    {
        private Dictionary<string, List<object?>> _properties = new Dictionary<string, List<object?>>(StringComparer.OrdinalIgnoreCase);
        private bool _disposedValue;

        // Properties to track method calls for verification
        public bool CloseCalled { get; private set; }
        public bool CommitChangesCalled { get; private set; }
        public bool RefreshCacheCalled { get; private set; }
        public string? RenamedTo { get; private set; }
        public IDirectoryEntry? MovedToParent { get; private set; }
        public List<(string MethodName, object?[]? Args)> InvokedMethods { get; } = new List<(string, object?[]?)>();


        public MockDirectoryEntry(string name = "MockEntry", string path = "LDAP://OU=MockOU,DC=example,DC=com")
        {
            Name = name;
            Path = path;
            NativeGuid = Guid.NewGuid().ToString();
            SchemaClassName = "user"; // Default, can be changed
            AuthenticationType = AuthenticationTypes.None;
            UsePropertyCache = false;
            Parent = null; // Can be set in tests
            Children = new MockDirectoryEntries(this); // Pass current mock as potential parent
        }

        public string Path { get; set; }
        public string? NativeGuid { get; set; }
        public string SchemaClassName { get; set; }
        public string Name { get; set; }
        public IDirectoryEntry? Parent { get; set; }
        public IDirectoryEntries Children { get; set; }
        public AuthenticationTypes AuthenticationType { get; set; }
        public bool UsePropertyCache { get; set; }


        public void SetPropertyValue(string propertyName, object? value)
        {
            if (!_properties.ContainsKey(propertyName))
            {
                _properties[propertyName] = new List<object?>();
            }
            _properties[propertyName].Clear(); // .Value typically overwrites
            _properties[propertyName].Add(value);
        }

        public void RemovePropertyValue(string propertyName, object? value)
        {
            if (_properties.TryGetValue(propertyName, out var values))
            {
                values.Remove(value);
            }
        }

        public void AddPropertyValue(string propertyName, object? value)
        {
            if (!_properties.TryGetValue(propertyName, out var values))
            {
                values = new List<object?>();
                _properties[propertyName] = values;
            }
            values.Add(value);
        }

        public void ClearPropertyValue(string propertyName)
        {
            if (_properties.TryGetValue(propertyName, out var values))
            {
                values.Clear();
            }
        }

        public bool ContainsProperty(string propertyName)
        {
            return _properties.ContainsKey(propertyName);
        }

        public bool PropertyContains(string propertyName, object value)
        {
            return _properties.TryGetValue(propertyName, out var values) && values != null && values.Contains(value);
        }

        public object? GetPropertyValue(string propertyName)
        {
            var array  = GetPropertyValues(propertyName);
            if(array != null && array.Length > 1){
                return array;
            }
            // .Value behavior: returns the first value if collection, or the value itself.
            // If property doesn't exist or has no values, behavior can vary.
            // For mock, we'll return first or null.
            if (_properties.TryGetValue(propertyName, out var values) && values != null && values.Count > 0)
            {
                return values.FirstOrDefault();
            }
            // To more closely mimic DirectoryEntry, which might throw if property doesn't exist
            // after an initial load, you could throw here, or ensure tests set up properties first.
            // For simplicity, returning null if not found or empty.
            if (!_properties.ContainsKey(propertyName))
            {
                //This mimics the behavior of DirectoryServices where accessing a non-existent property
                //implicitly adds it to the Properties collection (though not necessarily with a value yet)
                //_properties[propertyName] = new List<object?>();
                //However, for a mock, it's often cleaner to expect properties to be explicitly set up if they need to be read.
                //If DirectoryServices throws, then this mock should ideally throw too.
                //Let's assume for now it returns null, which is a common behavior for .Value if the property exists but has no values.
            }
            return null;
        }

        /// <summary>
        /// Helper for testing to get all values of a multi-valued property.
        /// </summary>
        public object?[] GetPropertyValues(string propertyName)
        {
            if (_properties.TryGetValue(propertyName, out var values))
            {
                try
                {
                    var first = values[0];
                    var typee = first.GetType();
                    if(first!=null && first is List<object> list){
                        if (list.Count > 1)
                        {
                            return list.ToArray();
                        }
                    }
                   
                }
                catch
                {
                    //do nothing
                }
            }
            return default;
        }


        public void Close()
        {
            CloseCalled = true;
            // In a real scenario, this might release resources.
        }

        public void CommitChanges()
        {
            CommitChangesCalled = true;
            // In a real scenario, this would save changes to the directory.
        }

        public object Invoke(string methodName, params object?[]? args)
        {
            InvokedMethods.Add((methodName, args));
            // Return a default value or configure specific mock responses based on methodName
            Console.WriteLine($"MockInvoke: {methodName} with args: {(args != null ? string.Join(", ", args) : "null")}");
            if (methodName == "SetPassword") return null; // Example
            if (methodName == "ChangePassword") return null; // Example
            return new object(); // Placeholder
        }

        public void MoveTo(IDirectoryEntry newParent)
        {
            MovedToParent = newParent;
            Parent = newParent;
            // You might want to update the Path property here too
            if (newParent != null)
            {
                var newParentPath = newParent.Path.EndsWith("/") ? newParent.Path : newParent.Path + "/";
                Path = newParentPath + Name; // Simplified name part
            }
        }

        public void RefreshCache()
        {
            RefreshCacheCalled = true;
            // No actual cache to refresh in mock
        }

        public void Rename(string newName)
        {
            RenamedTo = newName;
            Name = newName; // Update the mock's name
            // Path might also need updating based on CN or RDN change
            if (Path.Contains(",")) // Basic check for DN
            {
                var parts = Path.Split(new[] { ',' }, 2);
                if (parts.Length > 1 && parts[0].StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
                {
                    Path = $"CN={newName},{parts[1]}";
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    _properties.Clear();
                    CloseCalled = true; // Often Close is called by Dispose
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class MockDirectoryEntries : IDirectoryEntries
    {
        private readonly List<IDirectoryEntry> _entries = new List<IDirectoryEntry>();
        private readonly MockDirectoryEntry _parentEntry; // Optional: for context

        public MockDirectoryEntries(MockDirectoryEntry parentEntry = null)
        {
            _parentEntry = parentEntry;
        }

        public IDirectoryEntry Add(string name, string schemaClassName)
        {
            var newEntryPath = _parentEntry != null ? $"{_parentEntry.Path}/{name}" : $"LDAP://CN={name},DC=mock";
            var newEntry = new MockDirectoryEntry(name, newEntryPath) { SchemaClassName = schemaClassName, Parent = _parentEntry };
            _entries.Add(newEntry);
            return newEntry;
        }

        public IDirectoryEntry Find(string name, string schemaClassName)
        {
            return _entries.FirstOrDefault(e =>
                (e.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) ?? false) &&
                (schemaClassName == null || e.SchemaClassName?.Equals(schemaClassName, StringComparison.OrdinalIgnoreCase) == true));
        }

        public IDirectoryEntry Find(string name)
        {
            return Find(name, null);
        }

        public void Remove(IDirectoryEntry entry)
        {
            _entries.Remove(entry);
        }

        public IEnumerator GetEnumerator()
        {
            return _entries.GetEnumerator();
        }

        // Helper for tests to inspect entries
        public List<IDirectoryEntry> GetEntries() => new List<IDirectoryEntry>(_entries);
    }
}
