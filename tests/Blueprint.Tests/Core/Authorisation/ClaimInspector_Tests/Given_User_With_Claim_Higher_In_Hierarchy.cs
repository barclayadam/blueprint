using System.Security.Claims;
using Blueprint.Core.Authorisation;
using Blueprint.Core.Caching;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using ClaimTypes = Blueprint.Core.Authorisation.ClaimTypes;

namespace Blueprint.Tests.Core.Authorisation.ClaimInspector_Tests
{
    public class Given_User_With_Claim_Higher_In_Hierarchy
    {
        [Test]
        public void When_Invoked_Then_Does_Not_Throw_SecurityException()
        {
            // Arrange
            var userClaim = new Claim(
                    ClaimTypes.Permission,
                    "Account/6",
                    "Read");

            var demandedClaim = new Claim(
                    ClaimTypes.Permission,
                    "Task/4",
                    "Read");

            var resourceKeyExpander = new Mock<IResourceKeyExpander>();
            resourceKeyExpander.Setup(e => e.Expand("Task/4")).Returns("Account/6/Project/9/Task/4");

            var claimChecker = new ClaimInspector(new[] { resourceKeyExpander.Object }, Cache.NoCache, new NullLogger<ClaimInspector>());

            // Act
            claimChecker.IsDemandedClaimFulfilled(
                                                  new[] { userClaim }.ToClaimsHolder(),
                                                  demandedClaim,
                                                  ClaimExpansionState.RequiresExpansion);

            // Assert
            Assert.Pass("No exception should be thrown when demanding required claims.");
        }
    }
}
