using System.Net;
using System.Threading.Tasks;
using Blueprint.Http;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Core
{
    public class Given_CSharp_Record
    {
        [Test]
        public async Task With_Object_Declared_And_OperationResult_Derived_Return_No_Wrapping()
        {
            // Arrange
            var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithOperation<OperationAsRecord>());

            var returnValue = new StatusCodeResult(HttpStatusCode.OK);

            // Act
            var result = await executor.ExecuteWithNewScopeAsync(new OperationAsRecord(returnValue));

            // Assert
            result.Should().Be(returnValue);
        }

        public record OperationAsRecord(StatusCodeResult Result) : ICommand<object>
        {
            public Task<object> Handle()
            {
                return Task.FromResult((object)Result);
            }
        }
    }
}
