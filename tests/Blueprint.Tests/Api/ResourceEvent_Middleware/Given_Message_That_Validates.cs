using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Api.Configuration;
using Blueprint.Core;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Api.ResourceEvent_Middleware
{
    public class Given_ResourceEventMiddleware
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
        public async Task When_ResourceCreated_And_Self_Query_Exists_Populates_Data()
        {
            // Arrange
            var executor = TestApiOperationExecutor.Create(o => o
                .WithOperation<CreationOperation>()
                .WithOperation<SelfQuery>()
                .Pipeline(p => p.AddResourceEvents<NullResourceEventRepository>()));

            // Act
            // Note, do 2 executions to ensure correct parameters and being passed around
            var result1 = await executor.ExecuteWithNewScopeAsync(new CreationOperation { IdToCreate = "1234" });
            var result2 = await executor.ExecuteWithNewScopeAsync(new CreationOperation { IdToCreate = "9876" });

            // Assert
            result1.ShouldBeContent<CreatedResourceEvent>().Data.Id.Should().Be("1234");
            result2.ShouldBeContent<CreatedResourceEvent>().Data.Id.Should().Be("9876");
        }

        [Test]
        public async Task When_ResourceCreated_Then_Attributes_Populated()
        {
            using (var t = SystemTime.PlayTimelord())
            {
                // Arrange
                var executor = TestApiOperationExecutor.Create(o => o
                    .WithOperation<CreationOperation>()
                    .WithOperation<SelfQuery>()
                    .Pipeline(p => p.AddResourceEvents<NullResourceEventRepository>()));

                // Act
                var result = await executor.ExecuteWithNewScopeAsync(new CreationOperation { IdToCreate = "1234" });

                // Assert
                var @event = result.ShouldBeContent<CreatedResourceEvent>();

                @event.Created.UtcDateTime.Should().BeCloseTo(t.UtcNow);
                @event.Href.Should().Be("/resources/1234");
                @event.Object.Should().Be("event");
                @event.Type.Should().Be("awesome.created");
                @event.ChangeType.Should().Be(ResourceEventChangeType.Created);
                @event.ResourceObject.Should().Be("awesome");
            }
        }
    }
}
