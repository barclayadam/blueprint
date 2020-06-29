using System.Linq;
using System.Security.Claims;
using Blueprint.Authorisation;
using Blueprint.Testing;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Blueprint.Tests.Core.Authorisation.ClaimInspector_Tests
{
    public class Given_Cached_Expanded_Claims
    {
        [Test]
        public void When_Same_ResourceKey_Different_Type_Then_Cache_Does_Not_Affect()
        {
            // Arrange
            var demandedClaim1 = new Claim(
                    "http://www.example.com/Claim/Permissions/Read",
                    "http://www.example.com/Claim/Resource/4",
                    "Type1");

            var demandedClaim2 = new Claim(
                    "http://www.example.com/Claim/Permissions/Read",
                    "http://www.example.com/Claim/Resource/4",
                    "Type2");

            var claimChecker = new ClaimInspector(Enumerable.Empty<IResourceKeyExpander>(), new InMemoryCache(), new NullLogger<ClaimInspector>());

            // Push through once to add to cache
            claimChecker.IsDemandedClaimFulfilled(
                                                  new[] { demandedClaim1 }.ToClaimsHolder(),
                                                  demandedClaim1,
                                                  ClaimExpansionState.RequiresExpansion);

            // Act

            var differentTypeIsValid = claimChecker.IsDemandedClaimFulfilled(
                                                                             new[] { demandedClaim1 }.ToClaimsHolder(),
                                                                             demandedClaim2,
                                                                             ClaimExpansionState.RequiresExpansion);

            // Assert
            differentTypeIsValid.Should().BeFalse();
        }
    }
}
