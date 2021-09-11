using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Blueprint.Authorisation;
using Blueprint.Configuration;
using Blueprint.Http;
using Blueprint.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Blueprint.Tests.ResourceEvents
{
    public class Given_Permissioned_GetById
    {
        [Test]
        public async Task When_Child_Operation_Has_Authorisation_Then_Skips_And_Allows()
        {
            // Arrange
            var executor = TestApiOperationExecutor.CreateHttp(
                o => o
                    .WithOperation<CreationOperation>()
                    .WithOperation<SelfQuery>()
                    .AddAuthentication(a => a.UseContextLoader<AnonymousUserAuthorisationContextFactory>())
                    .AddAuthorisation()
                    .AddResourceEvents<NullResourceEventRepository>(),
                s => s.AddSingleton<IClaimsIdentityProvider, NullClaimsIdentityProvider>());

            // Act
            var result = await executor.ExecuteAsync(new CreationOperation { IdToCreate = "1234" });

            // Assert
            result.ShouldBeContent<CreatedResourceEvent>().Data.Id.Should().Be("1234");
        }
        [AllowAnonymous]
        [RootLink("/some-static-value")]
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
        public class SelfQuery : IQuery<AwesomeApiResource>
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
            public CreatedResourceEvent(IQuery<AwesomeApiResource> selfQuery) : base(selfQuery)
            {
            }
        }

        public class AwesomeApiResource : ApiResource
        {
            public string Id { get; set; }
        }
    }
}
