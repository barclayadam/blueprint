using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Blueprint.Http;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;
using OpenTelemetry.Trace;

namespace Blueprint.Tests.Diagnostics;

public class Activity_Status_Tests
{
    [Test]
    public async Task When_No_Exception_Then_Status_Unset()
    {
        // Arrange
        var expected = new TestOperation();

        var handler = new TestApiOperationHandler<TestOperation>(new StatusCodeResult(HttpStatusCode.Accepted));
        var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithHandler(handler));

        using var activity = Activity.Current = new Activity("ExceptionTest").Start();

        // Act
        await executor.ExecuteAsync(expected);

        // Assert
        // We set to Unset, so that hosts can make decision based on the result. For example ASP.NET will look at the
        // final status code set. We will lean on that to allow returning error-related status codes (i.e. a 500 StatusCodeResult).
        activity.GetStatus().StatusCode.Should().Be(StatusCode.Unset);
    }
        
    [Test]
    public async Task When_Blueprint_ValidationException_Thrown_Then_Activity_Status_OK()
    {
        // Arrange
        var expected = new TestOperation();

        var handler = new TestApiOperationHandler<TestOperation>(new Blueprint.Validation.ValidationException("Validation failed"));
        var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithHandler(handler));

        using var activity = Activity.Current = new Activity("ExceptionTest").Start();

        // Act
        await executor.ExecuteWithNoUnwrapAsync(expected);

        // Assert
        activity.GetStatus().StatusCode.Should().Be(StatusCode.Ok);
    }
        
    [Test]
    public async Task When_Blueprint_ValidationException_Thrown_Then_No_Exception_Recorded()
    {
        // Arrange
        var expected = new TestOperation();

        var handler = new TestApiOperationHandler<TestOperation>(new Blueprint.Validation.ValidationException("Validation failed"));
        var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithHandler(handler));

        using var activity = Activity.Current = new Activity("ExceptionTest").Start();

        // Act
        await executor.ExecuteWithNoUnwrapAsync(expected);

        // Assert
        activity.Events.Should().BeEmpty();
    }
        
    [Test]
    public async Task When_DataAnnotations_ValidationException_Thrown_Then_Activity_Status_OK()
    {
        // Arrange
        var expected = new TestOperation();

        var handler = new TestApiOperationHandler<TestOperation>(new System.ComponentModel.DataAnnotations.ValidationException("Validation failed"));
        var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithHandler(handler));

        using var activity = Activity.Current = new Activity("ExceptionTest").Start();

        // Act
        await executor.ExecuteWithNoUnwrapAsync(expected);

        // Assert
        activity.GetStatus().StatusCode.Should().Be(StatusCode.Ok);
    }
        
    [Test]
    [TestCase(200, StatusCode.Ok)]
    [TestCase(301, StatusCode.Ok)]
    [TestCase(402, StatusCode.Ok)]
    [TestCase(500, StatusCode.Error)]
    [TestCase(501, StatusCode.Error)]
    [TestCase(502, StatusCode.Error)]
    public async Task When_Blueprint_ApiException_Activity_Status_Set_Based_On_Http_Status_Code(int status, StatusCode expected)
    {
        // Arrange
        var handler = new TestApiOperationHandler<TestOperation>(new ApiException("Failed", "test_failure", "Test failure for " + status, status));
        var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithHandler(handler));

        using var activity = Activity.Current = new Activity("ExceptionTest").Start();

        // Act
        await executor.ExecuteWithNoUnwrapAsync(new TestOperation());

        // Assert
        activity.GetStatus().StatusCode.Should().Be(expected);
    }
        
    [Test]
    [TestCase(typeof(InvalidOperationException))]
    [TestCase(typeof(NullReferenceException))]
    public async Task When_Unhandled_Exception_Activity_Status_Set_To_Error(Type exceptionType)
    {
        // Arrange
        var handler = new TestApiOperationHandler<TestOperation>((Exception)Activator.CreateInstance(exceptionType, "The exception message"));
        var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithHandler(handler));

        using var activity = Activity.Current = new Activity("ExceptionTest").Start();

        // Act
        await executor.ExecuteWithNoUnwrapAsync(new TestOperation());

        // Assert
        activity.GetStatus().StatusCode.Should().Be(StatusCode.Error);
        activity.GetStatus().Description.Should().Be("The exception message");
    }
        
    [Test]
    [TestCase(typeof(InvalidOperationException))]
    [TestCase(typeof(NullReferenceException))]
    public async Task When_Unhandled_Exception_Activity_Exception_Recorded(Type exceptionType)
    {
        // Arrange
        var handler = new TestApiOperationHandler<TestOperation>((Exception)Activator.CreateInstance(exceptionType));
        var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithHandler(handler));

        using var activity = Activity.Current = new Activity("ExceptionTest").Start();

        // Act
        await executor.ExecuteWithNoUnwrapAsync(new TestOperation());

        // Assert
        activity.Events.Should().Contain(e => 
            e.Name == "exception" && 
            e.Tags.Any(t => t.Key == "exception.type"));
    }

    public class TestOperation : IQuery
    {
    }
}