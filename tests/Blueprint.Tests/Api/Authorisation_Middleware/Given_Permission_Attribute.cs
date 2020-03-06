using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Authorisation;
using Blueprint.Api.Configuration;
using Blueprint.Api.Errors;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;
using ClaimTypes = Blueprint.Core.Authorisation.ClaimTypes;

namespace Blueprint.Tests.Api.Authorisation_Middleware
{
    public class Given_Permission_Attribute
    {
        [Permission("ExecuteThisOperation")]
        public class ClaimRequiredOperation : IApiOperation
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
            var executor = TestApiOperationExecutor.Create(o => o
                .WithOperation<ClaimRequiredOperation>()
                .Pipeline(p => p.AddAuth<TestUserAuthorisationContextFactory>()));

            // Act
            var result = await executor.ExecuteWithAuth(new ClaimRequiredOperation());

            // Assert
            var okResult = result.Should().BeOfType<UnhandledExceptionOperationResult>().Subject;

            okResult.Exception.Should().BeOfType<ForbiddenException>();
            okResult.Exception.Message.Should().Be("User does not have required claim urn:claims/permission ExecuteThisOperation for *");
        }

        [Test]
        public async Task When_User_Does_Have_Required_Claim_Then_Exception()
        {
            // Arrange
            var executor = TestApiOperationExecutor.Create(o => o
                .WithOperation<ClaimRequiredOperation>()
                .Pipeline(p => p
                    .AddAuth<TestUserAuthorisationContextFactory>()));

            // Act
            var result = await executor.ExecuteWithAuth(
                new ClaimRequiredOperation(),
                new Claim(ClaimTypes.Permission, "*", "ExecuteThisOperation"));

            // Assert
            var okResult = result.Should().BeOfType<OkResult>().Subject;
            okResult.Content.Should().Be("12345");
        }
    }
}
