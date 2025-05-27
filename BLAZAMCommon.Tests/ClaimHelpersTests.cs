using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using BLAZAM.Common.Data; // For UserRoles
using BLAZAM.Helpers; // For ClaimHelpers extension methods
using Xunit;

namespace BLAZAM.Common.Tests
{
    public class ClaimHelpersTests
    {
        [Fact]
        public void AddSuperAdmin_ShouldAddSuperAdminClaim()
        {
            // Arrange
            var claims = new List<Claim>();

            // Act
            claims.AddSuperAdmin();

            // Assert
            Assert.Single(claims);
            var claim = claims.First();
            Assert.Equal(ClaimTypes.Role, claim.Type);
            Assert.Equal(UserRoles.SuperAdmin, claim.Value);
        }

        [Fact]
        public void AddSuperAdmin_NullList_ShouldThrowArgumentNullException()
        {
            // Arrange
            IList<Claim> claims = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => claims.AddSuperAdmin());
        }

        [Fact]
        public void AddAllRoles_ShouldAddAllUserRoles()
        {
            // Arrange
            var claims = new List<Claim>();
            var expectedRoles = UserRoles.All;

            // Act
            claims.AddAllRoles();

            // Assert
            Assert.Equal(expectedRoles.Count, claims.Count);
            foreach (var role in expectedRoles)
            {
                Assert.Contains(claims, c => c.Type == ClaimTypes.Role && c.Value == role);
            }
        }
        
        [Fact]
        public void AddAllRoles_ShouldNotAddSuperAdminRole()
        {
            // Arrange
            var claims = new List<Claim>();

            // Act
            claims.AddAllRoles();

            // Assert
            Assert.DoesNotContain(claims, c => c.Type == ClaimTypes.Role && c.Value == UserRoles.SuperAdmin);
        }


        [Fact]
        public void AddAllRoles_NullList_ShouldThrowArgumentNullException()
        {
            // Arrange
            IList<Claim> claims = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => claims.AddAllRoles());
        }
    }
}
