using System.Security.Claims;
using System.Threading.Tasks;
using Blueprint.Authorisation;
using Blueprint.Configuration;
using Blueprint.Errors;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;
using ClaimTypes = Blueprint.Authorisation.ClaimTypes;

namespace Blueprint.Tests.Authorisation
{
    public class Given_Permission_Attribute
    {
        [Permission("ExecuteThisOperation")]
        public class ClaimRequiredOperation
        {
            public object Invoke()
            {
                return "12345";
            }
        }

        [Test]
        public async Task When_User_Does_Not_Have_Required_Claim_Then_Exception()
        {
            // Arrange
            var executor = TestApiOperationExecutor.CreateStandalone(o => o
                .WithOperation<ClaimRequiredOperation>()
                .AddAuthentication(a => a.UseContextLoader<TestUserAuthorisationContextFactory>())
                .AddAuthorisation());

            // Act
            var result = await executor.ExecuteWithAuth(new ClaimRequiredOperation());

            // Assert
            var okResult = result.Should().BeOfType<UnhandledExceptionOperationResult>().Subject;

            okResult.Exception.Should().BeOfType<ForbiddenException>();
            ((ForbiddenException)okResult.Exception).Detail.Should().Be("User does not have required claim urn:claims/permission ExecuteThisOperation for *");
            ((ForbiddenException)okResult.Exception).Message.Should().Be("User does not have required claim urn:claims/permission ExecuteThisOperation for *");
            ((ForbiddenException)okResult.Exception).Title.Should().Be("You do not have enough permissions to perform this action");
        }

        [Test]
        public async Task When_User_Does_Have_Required_Claim_Then_Exception()
        {
            // Arrange
            var executor = TestApiOperationExecutor.CreateStandalone(o => o
                .WithOperation<ClaimRequiredOperation>()
                .AddAuthentication(a => a.UseContextLoader<TestUserAuthorisationContextFactory>())
                .AddAuthorisation());

            // Act
            var result = await executor.ExecuteWithAuth(
                new ClaimRequiredOperation(),
                new Claim(ClaimTypes.Permission, "*", "ExecuteThisOperation"));

            // Assert
            var okResult = result.ShouldBeOperationResultType<OkResult>();
            okResult.Content.Should().Be("12345");
        }
    }
}
