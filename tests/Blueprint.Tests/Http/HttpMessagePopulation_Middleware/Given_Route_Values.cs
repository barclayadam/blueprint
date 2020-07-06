using System;
using System.Threading.Tasks;
using Blueprint.Configuration;
using Blueprint.Http;
using Blueprint.Testing;
using Blueprint.Tests.Api;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Blueprint.Tests.Http.HttpMessagePopulation_Middleware
{
    public class Given_Route_Values
    {
        public enum OperationEnum
        {
            EnumOne,
            EnumTwo
        }

        public interface RouteableOperation<T> : IApiOperation
        {
            T RouteProperty { get; set; }
        }

        [RootLink("/string/{RouteProperty}")]
        public class StringOperation : RouteableOperation<string>
        {
            public string RouteProperty { get; set; }
        }

        [RootLink("/multiple1/{RouteProperty}")]
        [RootLink("/multiple2/{RouteProperty}")]
        public class MultipleRouteSamePropertyOperation : RouteableOperation<string>
        {
            public string RouteProperty { get; set; }
        }

        [RootLink("/string/{RouteProperty:ADifferentName}")]
        public class StringOperationWithAlternativeName : RouteableOperation<string>
        {
            public string RouteProperty { get; set; }
        }

        [RootLink("/string/{routeproperty}")]
        public class IncorrectCasedRoutePropertyOperation : RouteableOperation<string>
        {
            public string RouteProperty { get; set; }
        }

        [RootLink("/integer/{RouteProperty}")]
        public class IntegerOperation : RouteableOperation<int>
        {
            public int RouteProperty { get; set; }
        }

        [RootLink("/nullable-integer/{RouteProperty}")]
        public class NullableIntegerOperation : RouteableOperation<int?>
        {
            public int? RouteProperty { get; set; }
        }

        [RootLink("/guid/{RouteProperty}")]
        public class GuidOperation : RouteableOperation<Guid>
        {
            public Guid RouteProperty { get; set; }
        }

        [RootLink("/enum/{RouteProperty}")]
        public class EnumOperation : RouteableOperation<OperationEnum>
        {
            public OperationEnum RouteProperty { get; set; }
        }

        [Test]
        public async Task When_String_Property_Then_Populates()
        {
            // Arrange
            await AssertPopulatedFromRoute<StringOperation, string>(new StringOperation
            {
                RouteProperty = "expected-route-value"
            });
        }

        [Test]
        public async Task When_String_Property_With_Alternate_Name_Then_Populates()
        {
            // Arrange
            await AssertPopulatedFromRoute<StringOperationWithAlternativeName, string>(new StringOperationWithAlternativeName
            {
                RouteProperty = "expected-route-value"
            });
        }

        [Test]
        public async Task When_Multiple_Routes_With_Same_Property_Then_Populates()
        {
            // Arrange
            await AssertPopulatedFromRoute<MultipleRouteSamePropertyOperation, string>(new MultipleRouteSamePropertyOperation()
            {
                RouteProperty = "the-value-in-route"
            });
        }

        [Test]
        public async Task When_Integer_Property_Then_Populates()
        {
            // Arrange
            await AssertPopulatedFromRoute<IntegerOperation, int>(new IntegerOperation
            {
                RouteProperty = 1548
            });
        }

        [Test]
        public async Task When_Nullable_Integer_Property_Then_Populates()
        {
            // Arrange
            await AssertPopulatedFromRoute<NullableIntegerOperation, int?>(new NullableIntegerOperation
            {
                RouteProperty = 5481
            });
        }

        [Test]
        public async Task When_Guid_Property_Then_Populates()
        {
            // Arrange
            await AssertPopulatedFromRoute<GuidOperation, Guid>(new GuidOperation
            {
                RouteProperty = Guid.NewGuid()
            });
        }

        [Test]
        public async Task When_Enum_Property_Then_Populates()
        {
            // Arrange
            await AssertPopulatedFromRoute<EnumOperation, OperationEnum>(new EnumOperation
            {
                RouteProperty = OperationEnum.EnumTwo
            });
        }

        [Test]
        public async Task When_Incorrectly_Cased_Property_Then_Populates()
        {
            // We override the route data key to mimic the real behaviour of routing which would be to give the RouteData key as
            // the value in the route definition (i.e. NOT necessarily the name of the operation's property).
            await AssertPopulatedFromRoute<IncorrectCasedRoutePropertyOperation, string>(new IncorrectCasedRoutePropertyOperation
            {
                RouteProperty = "expected-route-value"
            });
        }

        private static async Task AssertPopulatedFromRoute<TOperation, TPropertyType>(TOperation expected, string routeDataKeyOverride = null) where TOperation : RouteableOperation<TPropertyType>
        {
            // Arrange
            var handler = new TestApiOperationHandler<TOperation>(null);
            var executor = TestApiOperationExecutor.Create(o => o.WithHandler(handler).Configure(p => p.AddHttp()));

            // These tests are checking whether a conversion happens, so we will always put the route data as the ToString() value of the object
            var context = GetContext<TOperation>(executor, routeDataKeyOverride ?? nameof(expected.RouteProperty), expected.RouteProperty.ToString());

            // Act
            await executor.ExecuteAsync(context);

            // Assert
            handler.OperationPassed.Should().BeEquivalentTo(expected);
        }

        private static ApiOperationContext GetContext<T>(TestApiOperationExecutor executor, string routeKey, string routeValue) where T : IApiOperation
        {
            var context = executor.HttpContextFor<T>();
            context.GetHttpContext().Request.Headers["Content-Type"] = "application/json";
            context.GetRouteData().Values[routeKey] = routeValue;

            return context;
        }
    }
}
