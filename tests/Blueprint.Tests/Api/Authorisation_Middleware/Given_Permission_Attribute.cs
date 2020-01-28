using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Authorisation;
using Blueprint.Api.Configuration;
using Blueprint.Api.Http;
using Blueprint.Core.Authorisation;
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
                .Pipeline(p => p
                    .AddAuth<TestUserAuthorisationContextFactory>()));

            // Act
            var result = await executor.ExecuteWithAuth(new ClaimRequiredOperation());

            // Assert
            var okResult = result.Should().BeOfType<UnhandledExceptionOperationResult>().Subject;
            okResult.Content.Error.Code.Should().Be("unauthorized");
            okResult.Content.Error.Message.Should().Be("You do not have enough permissions to perform this action");
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

        private class TestUserAuthorisationContextFactory : IUserAuthorisationContextFactory
        {
            public Task<IUserAuthorisationContext> CreateContextAsync(ClaimsIdentity claimsIdentity)
            {
                return Task.FromResult((IUserAuthorisationContext) new TestUserAuthorisationContext
                {
                    Claims = claimsIdentity.Claims.ToList()
                });
            }
        }

        public class TestUserAuthorisationContext : IUserAuthorisationContext, IClaimsHolder
        {
            public List<Claim> Claims { get; set; } = new List<Claim>();

            public bool IsActive { get; set; } = true;

            public bool IsAnonymous { get; set; } = false;

            public string Id { get; set; } = "TestUser123";

            public string AccountId { get; set; } = "TestAccount456";

            public string Email { get; set; }

            public string Name { get; }

            public void PopulateMetadata(Action<string, object> add)
            {
            }

            public IEnumerable<Claim> GetClaimsByValueType(string valueType)
            {
                return Claims.FindAll(c => c.ValueType == valueType);
            }
        }
    }

    public static class AuthBuilderExtensions
    {
        public static Task<OperationResult> ExecuteWithAuth(this TestApiOperationExecutor executor, IApiOperation operation, params Claim[] claims)
        {
            var context = executor.ContextFor(operation);

            context.ClaimsIdentity = new ClaimsIdentity(claims, "TestAuth");

            return executor.ExecuteAsync(context);
        }
    }
}
