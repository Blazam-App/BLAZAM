using System.Security.Claims;
using BLAZAM.Common.Data;
using BLAZAM.Database.Models.Notifications;
using BLAZAM.Database.Models.Permissions;
using BLAZAM.Database.Models.User;
using BLAZAM.Global.Events;
using BLAZAM.Session;
using BLAZAM.Session.Interfaces;
using Microsoft.AspNetCore.Authentication;

namespace BLAZAM.Tests.Session
{
    public class SessionTests
    {
        [Fact]
        public void LocalAuthentication_Constant_ShouldHaveCorrectValue()
        {
            // Arrange
            string expectedValue = "Local Authentication";

            // Act
            string actualValue = AppAuthenticationTypes.LocalAuthentication;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void ActiveDirectoryAuthentication_Constant_ShouldHaveCorrectValue()
        {
            // Arrange
            string expectedValue = "Active Directory Authentication";

            // Act
            string actualValue = AppAuthenticationTypes.ActiveDirectoryAuthentication;

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void SetAndGet_ByTypeKey_ShouldStoreAndRetrieveObject()
        {
            // Arrange
            IApplicationUserSessionCache cache = new ApplicationUserSessionCache();
            var key = typeof(MyCacheableItem);
            var expectedItem = new MyCacheableItem(1, "Test Item");

            // Act
            cache.Set(key, expectedItem);
            var retrievedItem = cache.Get<MyCacheableItem>(key);

            // Assert
            Assert.NotNull(retrievedItem);
            Assert.Same(expectedItem, retrievedItem); // Checks for reference equality
            Assert.Equal(expectedItem.Id, retrievedItem.Id);
            Assert.Equal(expectedItem.Name, retrievedItem.Name);
        }

        [Fact]
        public void Get_ByTypeKey_WhenKeyNotFound_ShouldReturnNewInstance()
        {
            // Arrange
            IApplicationUserSessionCache cache = new ApplicationUserSessionCache();
            var key = typeof(MyCacheableItem);

            // Act
            var retrievedItem = cache.Get<MyCacheableItem>(key);

            // Assert
            Assert.NotNull(retrievedItem);
            // For a new T(), properties will have default values (0 for int, null for string)
            Assert.Equal(0, retrievedItem.Id);
            Assert.Null(retrievedItem.Name);
        }

        [Fact]
        public void Set_ByTypeKey_WhenKeyExists_ShouldUpdateValue()
        {
            // Arrange
            IApplicationUserSessionCache cache = new ApplicationUserSessionCache();
            var key = typeof(MyCacheableItem);
            var initialItem = new MyCacheableItem(1, "Initial");
            var updatedItem = new MyCacheableItem(2, "Updated");

            // Act
            cache.Set(key, initialItem);
            cache.Set(key, updatedItem); // Update with new item
            var retrievedItem = cache.Get<MyCacheableItem>(key);

            // Assert
            Assert.NotNull(retrievedItem);
            Assert.Same(updatedItem, retrievedItem);
            Assert.Equal(updatedItem.Id, retrievedItem.Id);
            Assert.Equal(updatedItem.Name, retrievedItem.Name);
        }

        // --- String-Keyed Cache Tests ---

        [Fact]
        public void SetAndGet_ByStringKey_ShouldStoreAndRetrieveObject()
        {
            // Arrange
            IApplicationUserSessionCache cache = new ApplicationUserSessionCache();
            string key = "MyCustomStringKey";
            var expectedItem = new MyCacheableItem(10, "String Key Item");

            // Act
            cache.Set(key, expectedItem);
            var retrievedItem = cache.Get<MyCacheableItem>(key);

            // Assert
            Assert.NotNull(retrievedItem);
            Assert.Same(expectedItem, retrievedItem); // Checks for reference equality
            Assert.Equal(expectedItem.Id, retrievedItem.Id);
            Assert.Equal(expectedItem.Name, retrievedItem.Name);
        }

        [Fact]
        public void Get_ByStringKey_WhenKeyNotFound_ShouldReturnNewInstance()
        {
            // Arrange
            IApplicationUserSessionCache cache = new ApplicationUserSessionCache();
            string key = "NonExistentStringKey";

            // Act
            var retrievedItem = cache.Get<MyCacheableItem>(key);

            // Assert
            Assert.NotNull(retrievedItem);
            Assert.Equal(0, retrievedItem.Id);
            Assert.Null(retrievedItem.Name);
        }

        [Fact]
        public void Set_ByStringKey_WhenKeyExists_ShouldUpdateValue()
        {
            // Arrange
            IApplicationUserSessionCache cache = new ApplicationUserSessionCache();
            string key = "MyUpdatableStringKey";
            var initialItem = new MyCacheableItem(100, "Initial String");
            var updatedItem = new MyCacheableItem(200, "Updated String");

            // Act
            cache.Set(key, initialItem);
            cache.Set(key, updatedItem); // Update
            var retrievedItem = cache.Get<MyCacheableItem>(key);

            // Assert
            Assert.NotNull(retrievedItem);
            Assert.Same(updatedItem, retrievedItem);
            Assert.Equal(updatedItem.Id, retrievedItem.Id);
            Assert.Equal(updatedItem.Name, retrievedItem.Name);
        }

        [Fact]
        public void Get_ByTypeKey_WhenItemIsValueType_ShouldStoreAndRetrieveValue()
        {
            // Arrange
            IApplicationUserSessionCache cache = new ApplicationUserSessionCache();
            var key = typeof(int);
            int expectedValue = 12345;

            // Act
            cache.Set(key, expectedValue);
            var retrievedValue = cache.Get<int>(key); // int has a parameterless constructor

            // Assert
            Assert.Equal(expectedValue, retrievedValue);
        }

        [Fact]
        public void Get_ByStringKey_WhenItemIsValueType_ShouldStoreAndRetrieveValue()
        {
            // Arrange
            IApplicationUserSessionCache cache = new ApplicationUserSessionCache();
            string key = "MyIntValueKey";
            int expectedValue = 67890;

            // Act
            cache.Set(key, expectedValue);
            var retrievedValue = cache.Get<int>(key);

            // Assert
            Assert.Equal(expectedValue, retrievedValue);
        }
        [Fact]
        public void MFARequest_Constructor_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            string expectedToken = "test-token-123";
            string expectedRedirectUrl = "/redirect/path";
            var mockUser = new MockApplicationUserState();

            // Act
            var mfaRequest = new MFARequest(expectedToken, expectedRedirectUrl, mockUser);

            // Assert
            Assert.Equal(expectedToken, mfaRequest.mfaToken);
            Assert.Equal(expectedRedirectUrl, mfaRequest.redirectUrl);
            Assert.Same(mockUser, mfaRequest.user); // Check reference equality for the user object
        }

        [Theory]
        [InlineData("token1", "/url1", "token1", "/url2", true)] // Same token, different URL
        [InlineData("token1", "/url1", "token2", "/url1", false)] // Different token, same URL
        [InlineData("token1", "/url1", "token1", "/url1", true)] // Identical
        public void MFARequest_Equals_Object_ShouldCompareBasedOnToken(
            string tokenA, string urlA, string tokenB, string urlB, bool expectedEquality)
        {
            // Arrange
            var userA = new MockApplicationUserState();
            var userB = new MockApplicationUserState();
            var requestA = new MFARequest(tokenA, urlA, userA);
            var requestB = new MFARequest(tokenB, urlB, userB);

            // Act
            bool actualEquality = requestA.Equals((object)requestB);

            // Assert
            Assert.Equal(expectedEquality, actualEquality);
        }

        [Fact]
        public void MFARequest_Equals_Object_WithNull_ShouldReturnFalse()
        {
            // Arrange
            var requestA = new MFARequest("token", "/url", new MockApplicationUserState());

            // Act
            bool actualEquality = requestA.Equals((object?)null);

            // Assert
            Assert.False(actualEquality);
        }

        [Fact]
        public void MFARequest_Equals_Object_WithDifferentType_ShouldReturnFalse()
        {
            // Arrange
            var requestA = new MFARequest("token", "/url", new MockApplicationUserState());
            var differentObject = new object();

            // Act
            bool actualEquality = requestA.Equals(differentObject);

            // Assert
            Assert.False(actualEquality);
        }


        [Theory]
        [InlineData("token1", "/url1", "token1", "/url2", true)] // Same token
        [InlineData("token1", "/url1", "token2", "/url1", false)] // Different token
        public void MFARequest_Equals_MFARequest_ShouldCompareBasedOnToken(
            string tokenA, string urlA, string tokenB, string urlB, bool expectedEquality)
        {
            // Arrange
            var userA = new MockApplicationUserState();
            var userB = new MockApplicationUserState();
            var requestA = new MFARequest(tokenA, urlA, userA);
            var requestB = new MFARequest(tokenB, urlB, userB);

            // Act
            bool actualEquality = requestA.Equals(requestB);

            // Assert
            Assert.Equal(expectedEquality, actualEquality);
        }

        [Fact]
        public void MFARequest_Equals_MFARequest_WithNull_ShouldReturnFalse()
        {
            // Arrange
            var requestA = new MFARequest("token", "/url", new MockApplicationUserState());

            // Act
            bool actualEquality = requestA.Equals(null);

            // Assert
            Assert.False(actualEquality);
        }

        [Fact]
        public void MFARequest_GetHashCode_ShouldBeEqualForEqualObjects()
        {
            // Arrange
            var user = new MockApplicationUserState();
            var requestA = new MFARequest("same-token", "/urlA", user);
            var requestB = new MFARequest("same-token", "/urlB", user); // Different URL, but same token

            // Act & Assert
            Assert.Equal(requestA.GetHashCode(), requestB.GetHashCode());
        }

        [Fact]
        public void MFARequest_GetHashCode_ShouldLikelyBeDifferentForDifferentTokens()
        {
            // Arrange
            var user = new MockApplicationUserState();
            var requestA = new MFARequest("token-A", "/url", user);
            var requestB = new MFARequest("token-B", "/url", user);

            // Act & Assert
            // Note: Hash code collisions are possible but unlikely for simple string differences.
            // This test primarily checks that the hash code isn't a constant.
            if (requestA.mfaToken != requestB.mfaToken) // Ensure tokens are actually different
            {
                Assert.NotEqual(requestA.GetHashCode(), requestB.GetHashCode());
            }
        }

        [Theory]
        [InlineData("token1", "token1", true)]  // Same token
        [InlineData("token1", "token2", false)] // Different token
        public void MFARequest_EqualityOperator_ShouldCompareBasedOnToken(
            string tokenA, string tokenB, bool expectedEquality)
        {
            // Arrange
            var user = new MockApplicationUserState();
            var requestA = new MFARequest(tokenA, "/url", user);
            var requestB = new MFARequest(tokenB, "/url", user);

            // Act
            bool actualEquality = (requestA == requestB);

            // Assert
            Assert.Equal(expectedEquality, actualEquality);
        }

        [Fact]
        public void MFARequest_EqualityOperator_BothNull_ShouldReturnTrue()
        {
            // Arrange
            MFARequest? requestA = null;
            MFARequest? requestB = null;

            // Act & Assert
            Assert.True(requestA == requestB);
        }

        [Fact]
        public void MFARequest_EqualityOperator_OneNull_ShouldReturnFalse()
        {
            // Arrange
            MFARequest? requestA = new MFARequest("token", "/url", new MockApplicationUserState());
            MFARequest? requestB = null;

            // Act & Assert
            Assert.False(requestA == requestB);
            Assert.False(requestB == requestA);
        }


        [Theory]
        [InlineData("token1", "token1", false)] // Same token -> not unequal
        [InlineData("token1", "token2", true)]  // Different token -> unequal
        public void MFARequest_InequalityOperator_ShouldCompareBasedOnToken(
            string tokenA, string tokenB, bool expectedInequality)
        {
            // Arrange
            var user = new MockApplicationUserState();
            var requestA = new MFARequest(tokenA, "/url", user);
            var requestB = new MFARequest(tokenB, "/url", user);

            // Act
            bool actualInequality = (requestA != requestB);

            // Assert
            Assert.Equal(expectedInequality, actualInequality);
        }


        [Fact]
        public void MFARequest_InequalityOperator_BothNull_ShouldReturnFalse()
        {
            // Arrange
            MFARequest? requestA = null;
            MFARequest? requestB = null;

            // Act & Assert
            Assert.False(requestA != requestB);
        }

        [Fact]
        public void MFARequest_InequalityOperator_OneNull_ShouldReturnTrue()
        {
            // Arrange
            MFARequest? requestA = new MFARequest("token", "/url", new MockApplicationUserState());
            MFARequest? requestB = null;

            // Act & Assert
            Assert.True(requestA != requestB);
            Assert.True(requestB != requestA);
        }
    }
    // --- Mock/Stub for IApplicationUserState (Helper for MFARequest tests) ---
    public class MockApplicationUserState : IApplicationUserState
    {
        // This is a minimal mock. Add properties or methods if MFARequest
        // interacts with IApplicationUserState in ways that need to be controlled during tests.
        // For the current MFARequest class, it's primarily just being stored.

        public int Id => throw new NotImplementedException();

        public AppDelegate OnSettingsChanged { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string AuditUsername => throw new NotImplementedException();

        public string? Username { get; set; } = "test";

        public ClaimsPrincipal? Impersonator { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsSuperAdmin => throw new NotImplementedException();

        public DateTime LastAccessed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ClaimsPrincipal User { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public AppUser Preferences => throw new NotImplementedException();

        public AuthenticationTicket? Ticket { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IApplicationUserSessionCache Cache { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string? IPAddress { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string LastUri { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsAuthenticated => throw new NotImplementedException();

        public List<PermissionDelegate> PermissionDelegates { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<PermissionMapping> PermissionMappings { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<NotificationSubscription> NotificationSubscriptions { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool HasUserPrivilege => throw new NotImplementedException();

        public bool HasCreateUserPrivilege => throw new NotImplementedException();

        public bool HasGroupPrivilege => throw new NotImplementedException();

        public bool HasCreateGroupPrivilege => throw new NotImplementedException();

        public bool HasOUPrivilege => throw new NotImplementedException();

        public bool HasCreateOUPrivilege => throw new NotImplementedException();

        public bool HasComputerPrivilege => throw new NotImplementedException();

        public bool HasBitLockerPrivilege => throw new NotImplementedException();

        public bool CanUnlockUsers => throw new NotImplementedException();

        public bool CanAssign => throw new NotImplementedException();

        public string DuoAuthState { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IList<ReadNewsItem>? ReadNewsItems => throw new NotImplementedException();

        public bool CanSearchDisabled(ActiveDirectoryObjectType objectType)
        {
            throw new NotImplementedException();
        }

        public void GetUserSettingFromDB()
        {
            throw new NotImplementedException();
        }

        public bool HasActionPermission(string dnTarget, ObjectAction action, ActiveDirectoryObjectType objectType)
        {
            throw new NotImplementedException();
        }

        public bool HasPermission(string dnTarget, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>> allowSelector, Func<IEnumerable<PermissionMapping>, IEnumerable<PermissionMapping>>? denySelector, bool nestedSearch)
        {
            throw new NotImplementedException();
        }

        public bool HasRole(string role)
        {
            throw new NotImplementedException();
        }

        public Task<bool> MarkAllRead()
        {
            throw new NotImplementedException();
        }

        public Task<bool> MarkRead(UserNotification notification)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveAllUserSettings()
        {
            throw new NotImplementedException();
        }

        public Task SaveBasicUserPreferences()
        {
            throw new NotImplementedException();
        }

        public Task SaveDashboardWidgets()
        {
            throw new NotImplementedException();
        }

        public Task SaveReadNewsItems()
        {
            throw new NotImplementedException();
        }
    }

    // Dummy class for testing with Type keys and generic Get<T>
    public class MyCacheableItem
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Parameterless constructor required by the 'new()' constraint in Get<T>
        public MyCacheableItem() { }

        public MyCacheableItem(int id, string name)
        {
            Id = id;
            Name = name;
        }

        // Optional: Override Equals and GetHashCode if you plan to compare instances directly
        // For these tests, we'll compare properties or reference equality.
    }

}
