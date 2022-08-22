using System.Net;
using System.Threading.Tasks;
using Blueprint.Http;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core;

public class Given_Operation_Return_Types
{
    [Test]
    public async Task With_Object_Declared_And_OperationResult_Derived_Return_No_Wrapping()
    {
        // Arrange
        var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithOperation<OperationWithObjectReturn>());

        var returnValue = new StatusCodeResult(HttpStatusCode.OK);

        // Act
        var result = await executor.ExecuteWithNewScopeAsync(new OperationWithObjectReturn
        {
            Result = returnValue
        });

        // Assert
        result.Should().Be(returnValue);
    }

    [Test]
    public async Task With_ValueTaskOfObject_Declared_And_OperationResult_Derived_Return_No_Wrapping()
    {
        // Arrange
        var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithOperation<OperationWithValueTaskObjectReturn>());

        var returnValue = new StatusCodeResult(HttpStatusCode.OK);

        // Act
        var result = await executor.ExecuteWithNewScopeAsync(new OperationWithValueTaskObjectReturn
        {
            Result = returnValue
        });

        // Assert
        result.Should().Be(returnValue);
    }

    [Test]
    public async Task With_TaskOfObject_Declared_And_OperationResult_Derived_Return_No_Wrapping()
    {
        // Arrange
        var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithOperation<OperationWithTaskObjectReturn>());

        var returnValue = new StatusCodeResult(HttpStatusCode.OK);

        // Act
        var result = await executor.ExecuteWithNewScopeAsync(new OperationWithTaskObjectReturn
        {
            Result = returnValue
        });

        // Assert
        result.Should().Be(returnValue);
    }

    [Test]
    public async Task With_Object_Declared_And_NOT_OperationResult_Derived_Return_Wrapped()
    {
        // Arrange
        var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithOperation<OperationWithObjectReturn>());

        var returnValue = new object();

        // Act
        var result = await executor.ExecuteWithNewScopeAsync(new OperationWithObjectReturn
        {
            Result = returnValue
        });

        // Assert
        result.Should().BeOfType<OkResult>();
        result.As<OkResult>().Content.Should().Be(returnValue);
    }

    [Test]
    public async Task With_OperationResult_Declared_Then_No_Wrapping()
    {
        // Arrange
        var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithOperation<OperationWithOperationResultReturn>());

        var returnValue = new StatusCodeResult(HttpStatusCode.OK);

        // Act
        var result = await executor.ExecuteWithNewScopeAsync(new OperationWithOperationResultReturn
        {
            Result = returnValue
        });

        // Assert
        result.Should().Be(returnValue);
    }

    [Test]
    public async Task With_OperationResult_Derived_Declared_Then_No_Wrapping()
    {
        // Arrange
        var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithOperation<OperationWithDerivedOperationResultReturn>());

        var returnValue = new StatusCodeResult(HttpStatusCode.OK);

        // Act
        var result = await executor.ExecuteWithNewScopeAsync(new OperationWithDerivedOperationResultReturn
        {
            Result = returnValue
        });

        // Assert
        result.Should().Be(returnValue);
    }

    public class OperationWithObjectReturn : ICommand<object>
    {
        public object Result { get; set; }

        public object Handle()
        {
            return Result;
        }
    }

    public class OperationWithValueTaskObjectReturn : ICommand<object>
    {
        public object Result { get; set; }

        public ValueTask<object> Handle()
        {
            return new ValueTask<object>(Result);
        }
    }

    public class OperationWithTaskObjectReturn : ICommand<object>
    {
        public object Result { get; set; }

        public Task<object> Handle()
        {
            return Task.FromResult(Result);
        }
    }

    public class OperationWithOperationResultReturn
    {
        public OperationResult Result { get; set; }

        public OperationResult Handle()
        {
            return Result;
        }
    }

    public class OperationWithDerivedOperationResultReturn
    {
        public StatusCodeResult Result { get; set; }

        public StatusCodeResult Handle()
        {
            return Result;
        }
    }
}