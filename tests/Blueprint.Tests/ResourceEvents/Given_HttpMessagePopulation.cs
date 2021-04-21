using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Blueprint.Http;
using Blueprint.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Blueprint.Tests.ResourceEvents
{
    public class Given_HttpMessagePopulation
    {
        [RootLink("/some-static-value")]
        public class CreationOperation : ICommand
        {
            [Required] public string IdToCreate { get; set; }

            public CreatedResourceEvent Invoke()
            {
                return new CreatedResourceEvent(new AwesomeApiResource { Id = IdToCreate });
            }
        }

        // Pick any authorisation that we would not have
        [SelfLink(typeof(AwesomeApiResource), "/resources/{Id}")]
        public class SelfQuery : IQuery<AwesomeApiResource>
        {
            [Required] public string Id { get; set; }

            public AwesomeApiResource Invoke()
            {
                return new AwesomeApiResource { Id = Id };
            }
        }

        public class CreatedResourceEvent : ResourceCreated<AwesomeApiResource>
        {
            public CreatedResourceEvent(AwesomeApiResource data) : base(data)
            {
            }
        }

        public class AwesomeApiResource : ApiResource
        {
            public string Id { get; set; }
        }

        [Test]
        public async Task When_HttpMessagePopulation_Child_Operation_Then_Properties_Are_Not_Populated()
        {
            // Arrange
            var executor = TestApiOperationExecutor.CreateHttp(o => o
                .WithOperation<CreationOperation>()
                .WithOperation<SelfQuery>()
                .AddResourceEvents<NullResourceEventRepository>());

            // Act
            var context = executor.HttpContextFor(new CreationOperation { IdToCreate = "1234" });

            var result = await executor.ExecuteAsync(context);

            // Assert
            result.ShouldBeContent<CreatedResourceEvent>().Data.Id.Should().Be("1234");
        }
    }
}
