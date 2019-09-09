using System.IdentityModel.Claims;
using System.Linq;
using Blueprint.Core.Authorisation;
using Blueprint.Core.Caching;
using FluentAssertions;
using NUnit.Framework;
using Claim = System.Security.Claims.Claim;

namespace Blueprint.Tests.Core.Authorisation.ClaimInspector_Tests
{
    public class Given_User_With_Single_Permission_Granted_Which_Command_Requires
    {
        [Test]
        public void When_Invoked_Then_Does_Returns_True()
        {
            // Arrange
            var demandedClaim = new Claim(
                    "http://www.example.com/Claim/Permissions/Read",
                    "http://www.example.com/Claim/Resource/4",
                    Rights.PossessProperty);

            var claimChecker = new ClaimInspector(Enumerable.Empty<IResourceKeyExpander>(), Cache.NoCache);

            // Act
            var isFulfilled = claimChecker.IsDemandedClaimFulfilled(
                new[] { demandedClaim }.ToClaimsHolder(), 
                demandedClaim, 
                ClaimExpansionState.RequiresExpansion);

            // Assert
            isFulfilled.Should().BeTrue();
        }
    }
}