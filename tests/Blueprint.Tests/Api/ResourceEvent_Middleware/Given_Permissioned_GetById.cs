using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Authorisation;
using Blueprint.Api.Configuration;
using Blueprint.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Blueprint.Tests.Api.ResourceEvent_Middleware
{
    public class Given_Permissioned_GetById
    {
        [AllowAnonymous]
        public class CreationOperation : ICommand
        {
            [Required]
            public string IdToCreate { get; set; }

            public CreatedResourceEvent Invoke()
            {
                return new CreatedResourceEvent(new SelfQuery { Id = IdToCreate });
            }
        }

        // Pick any authorisation that we would not have
        [MustBeAuthenticated]
        [SelfLink(typeof(AwesomeApiResource), "/resources/{Id}")]
        public class SelfQuery : IQuery
        {
            [Required]
            public string Id { get; set; }

            public AwesomeApiResource Invoke()
            {
                return new AwesomeApiResource
                {
                    Id = Id
                };
            }
        }

        public class CreatedResourceEvent : ResourceCreated<AwesomeApiResource>
        {
            public CreatedResourceEvent(IApiOperation selfQuery) : base(selfQuery)
            {
            }
        }

        public class AwesomeApiResource : ApiResource
        {
            public string Id { get; set; }
        }

        [Test]
        public async Task When_Child_Operation_Has_Authorisation_Then_Skips_And_Allows()
        {
            // Arrange
            var executor = TestApiOperationExecutor.Create(o => o
                .WithOperation<CreationOperation>()
                .WithOperation<SelfQuery>()
                .WithServices(s => s.AddSingleton<IClaimsIdentityProvider, NullClaimsIdentityProvider>())
                .Pipeline(p => p
                    .AddAuth<AnonymousUserAuthorisationContextFactory>()
                    .AddResourceEvents<NullResourceEventRepository>()));

            // Act
            var result = await executor.ExecuteWithNewScopeAsync(new CreationOperation { IdToCreate = "1234" });

            // Assert
            result.ShouldBeContent<CreatedResourceEvent>().Data.Id.Should().Be("1234");
        }

        private class NullClaimsIdentityProvider : IClaimsIdentityProvider
        {
            public ClaimsIdentity Get(ApiOperationContext context)
            {
                return null;
            }
        }
    }
}
