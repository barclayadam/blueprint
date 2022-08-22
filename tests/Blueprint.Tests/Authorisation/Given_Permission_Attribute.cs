using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Blueprint.Authorisation;
using Blueprint.Configuration;
using Blueprint.Errors;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;
using ClaimTypes = Blueprint.Authorisation.ClaimTypes;

namespace Blueprint.Tests.Authorisation;

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
        Func<Task> tryExecute = () => executor.ExecuteWithAuth(new ClaimRequiredOperation());

        // Assert
        var forbiddenException = await tryExecute.Should().ThrowExactlyAsync<ForbiddenException>();

        forbiddenException.And.Detail.Should().Be("User does not have required claim urn:claims/permission ExecuteThisOperation for *");
        forbiddenException.And.Message.Should().Be("User does not have required claim urn:claims/permission ExecuteThisOperation for *");
        forbiddenException.And.Title.Should().Be("You do not have enough permissions to perform this action");
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