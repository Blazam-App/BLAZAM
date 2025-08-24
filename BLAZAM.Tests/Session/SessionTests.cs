using BLAZAM.Session;
using BLAZAM.Session.Interfaces;
using Moq; // Added Moq namespace

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
            ApplicationUserSessionCache cache = new ApplicationUserSessionCache();
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
            ApplicationUserSessionCache cache = new ApplicationUserSessionCache();
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
            ApplicationUserSessionCache cache = new ApplicationUserSessionCache();
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
            ApplicationUserSessionCache cache = new ApplicationUserSessionCache();
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
            ApplicationUserSessionCache cache = new ApplicationUserSessionCache();
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
            ApplicationUserSessionCache cache = new ApplicationUserSessionCache();
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
            ApplicationUserSessionCache cache = new ApplicationUserSessionCache();
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
            ApplicationUserSessionCache cache = new ApplicationUserSessionCache();
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
            // Replaced hard-coded mock with Moq
            var mockUser = new Mock<IApplicationUserState>();

            // Act
            // Use mockUser.Object to pass the mocked instance
            var mfaRequest = new MFARequest(expectedToken, expectedRedirectUrl, mockUser.Object);

            // Assert
            Assert.Equal(expectedToken, mfaRequest.mfaToken);
            Assert.Equal(expectedRedirectUrl, mfaRequest.redirectUrl);
            Assert.Same(mockUser.Object, mfaRequest.user); // Check reference equality for the user object
        }

        [Theory]
        [InlineData("token1", "/url1", "token1", "/url2", true)] // Same token, different URL
        [InlineData("token1", "/url1", "token2", "/url1", false)] // Different token, same URL
        [InlineData("token1", "/url1", "token1", "/url1", true)] // Identical
        public void MFARequest_Equals_Object_ShouldCompareBasedOnToken(
            string tokenA, string urlA, string tokenB, string urlB, bool expectedEquality)
        {
            // Arrange
            // Replaced hard-coded mocks with Moq
            var userA = new Mock<IApplicationUserState>().Object;
            var userB = new Mock<IApplicationUserState>().Object;
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
            var requestA = new MFARequest("token", "/url", new Mock<IApplicationUserState>().Object);

            // Act
            bool actualEquality = requestA.Equals((object?)null);

            // Assert
            Assert.False(actualEquality);
        }

        [Fact]
        public void MFARequest_Equals_Object_WithDifferentType_ShouldReturnFalse()
        {
            // Arrange
            var requestA = new MFARequest("token", "/url", new Mock<IApplicationUserState>().Object);
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
            var userA = new Mock<IApplicationUserState>().Object;
            var userB = new Mock<IApplicationUserState>().Object;
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
            var requestA = new MFARequest("token", "/url", new Mock<IApplicationUserState>().Object);

            // Act
            bool actualEquality = requestA.Equals(null);

            // Assert
            Assert.False(actualEquality);
        }

        [Fact]
        public void MFARequest_GetHashCode_ShouldBeEqualForEqualObjects()
        {
            // Arrange
            var user = new Mock<IApplicationUserState>().Object;
            var requestA = new MFARequest("same-token", "/urlA", user);
            var requestB = new MFARequest("same-token", "/urlB", user); // Different URL, but same token

            // Act & Assert
            Assert.Equal(requestA.GetHashCode(), requestB.GetHashCode());
        }

        [Fact]
        public void MFARequest_GetHashCode_ShouldLikelyBeDifferentForDifferentTokens()
        {
            // Arrange
            var user = new Mock<IApplicationUserState>().Object;
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
            var user = new Mock<IApplicationUserState>().Object;
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
            MFARequest? requestA = new MFARequest("token", "/url", new Mock<IApplicationUserState>().Object);
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
            var user = new Mock<IApplicationUserState>().Object;
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
            MFARequest? requestA = new MFARequest("token", "/url", new Mock<IApplicationUserState>().Object);
            MFARequest? requestB = null;

            // Act & Assert
            Assert.True(requestA != requestB);
            Assert.True(requestB != requestA);
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
    }
}
