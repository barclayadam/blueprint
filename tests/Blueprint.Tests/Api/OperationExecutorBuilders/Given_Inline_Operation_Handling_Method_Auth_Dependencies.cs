using System.Security.Claims;
using System.Threading.Tasks;
using Blueprint.Authorisation;
using Blueprint.Configuration;
using Blueprint.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Blueprint.Tests.Api.OperationExecutorBuilders
{
    public class Given_Inline_Operation_Handling_Method_Auth_Dependencies
    {
        [Test]
        public async Task When_Dependency_From_Operation_Context_And_IoC_Then_Injects()
        {
            // Arrange
            var operation = new InlineHandle();
            var executor = TestApiOperationExecutor.Create(o =>
            {
                o.WithServices(s =>
                {
                    s.AddSingleton<IClaimsIdentityProvider, NullClaimsIdentityProvider>();
                    s.AddTransient<IDependency, Dependency>();
                });
                o.WithOperation<InlineHandle>();
                o.Configure(p => p.AddAuth<AnonymousUserAuthorisationContextFactory>());
            });

            // Act
            await executor.ExecuteWithNewScopeAsync(operation);

            // Assert
            operation.Context.Should().NotBeNull();
            operation.Context.Operation.Should().Be(operation);

            operation.Dependency.Should().NotBeNull();
            operation.User.Should().NotBeNull();
        }

        public interface IDependency {}

        public class Dependency : IDependency {}

        public class InlineHandle : IApiOperation
        {
            public ApiOperationContext Context { get; set; }

            public IDependency Dependency { get; set; }

            public IUserAuthorisationContext User { get; set; }

            public OkResult Handle(ApiOperationContext context, IDependency dependency, IUserAuthorisationContext user)
            {
                Context = context;
                Dependency = dependency;
                User = user;

                return new OkResult(nameof(InlineHandle));
            }
        }

        private class NullClaimsIdentityProvider : IClaimsIdentityProvider
        {
            public ClaimsIdentity Get(ApiOperationContext context)
            {
                return new ClaimsIdentity("TestAuthType");
            }
        }
    }
}
