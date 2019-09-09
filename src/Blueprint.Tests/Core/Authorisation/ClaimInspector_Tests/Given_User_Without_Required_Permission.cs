using System.IdentityModel.Claims;
using System.Linq;
using Blueprint.Core.Authorisation;
using Blueprint.Core.Caching;
using FluentAssertions;
using NUnit.Framework;
using Claim = System.Security.Claims.Claim;

namespace Blueprint.Tests.Core.Authorisation.ClaimInspector_Tests
{
    public class Given_User_Without_Required_Permission
    {
        [Test]
        public void When_Invoked_Then_Throws_SecurityException()
        {
            // Arrange
            var demandedClaim = new Claim("http://www.example.com/Claim/Permissions/Read", "http://www.example.com/Claim/Resource/4", Rights.PossessProperty);

            var claimChecker = new ClaimInspector(Enumerable.Empty<IResourceKeyExpander>(), Cache.NoCache);

            // Act
            var result = claimChecker.IsDemandedClaimFulfilled(
                Enumerable.Empty<Claim>().ToClaimsHolder(), 
                demandedClaim, 
                ClaimExpansionState.RequiresExpansion);

            // Assert
            result.Should().BeFalse();
        }
    }
}