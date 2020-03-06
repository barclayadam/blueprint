using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Api.ResourceEvent_Middleware
{
    public class Given_HttpMessagePopulation
    {
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
        public async Task When_HttpMessagePopulation_Child_Operation_Then_Properties_Are_Not_Populated()
        {
            // Arrange
            var executor = TestApiOperationExecutor.Create(o => o
                .WithOperation<CreationOperation>()
                .WithOperation<SelfQuery>()
                .Configure(a => a.AddHttp())
                .Pipeline(p => p.AddResourceEvents<NullResourceEventRepository>()));

            // Act
            var context = executor.HttpContextFor<CreationOperation>();
            ((CreationOperation)context.Operation).IdToCreate = "1234";

            var result = await executor.ExecuteAsync(context);

            // Assert
            result.ShouldBeContent<CreatedResourceEvent>().Data.Id.Should().Be("1234");
        }
    }
}
