using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Blueprint.Configuration;
using Blueprint.Http;
using Blueprint.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Internal.Execution;

namespace Blueprint.Tests.ResourceEvents;

public class Given_ResourceCreated_With_SelfQuery_Result
{
    [Test]
    public async Task When_ResourceCreated_And_Self_Query_Exists_Populates_Data()
    {
        // Arrange
        var executor = TestApiOperationExecutor.CreateHttp(o => o
            .WithOperation<CreationOperation>()
            .WithOperation<SelfQuery>()
            .AddResourceEvents<NullResourceEventRepository>());

        // Act
        // Note, do 2 executions to ensure correct parameters and being passed around
        var result1 = await executor.ExecuteAsync(new CreationOperation { IdToCreate = "1234" });
        var result2 = await executor.ExecuteAsync(new CreationOperation { IdToCreate = "9876" });

        // Assert
        result1.ShouldBeContent<CreatedResourceEvent>().Data.Id.Should().Be("1234");
        result2.ShouldBeContent<CreatedResourceEvent>().Data.Id.Should().Be("9876");
    }

    [Test]
    public async Task When_LinkGeneration_Middleware_Added_After_Then_Links_Populated()
    {
        // Arrange
        var executor = TestApiOperationExecutor.CreateHttp(o => o
            .WithOperation<CreationOperation>()
            .WithOperation<SelfQuery>()
            .AddResourceEvents<NullResourceEventRepository>()
            .AddHateoasLinks());

        // Act
        var result1 = await executor.ExecuteAsync(new CreationOperation { IdToCreate = "1234" });

        // Assert
        var evt = result1.ShouldBeContent<CreatedResourceEvent>();
        evt.Data.Links["self"].Href.Should().EndWith("/resources/1234");
    }

    [Test]
    public async Task When_LinkGeneration_Middleware_Added_First_Then_Links_Populated()
    {
        // Arrange
        var executor = TestApiOperationExecutor.CreateHttp(o => o
            .WithOperation<CreationOperation>()
            .WithOperation<SelfQuery>()
            .AddHateoasLinks()
            .AddResourceEvents<NullResourceEventRepository>());

        // Act
        var result1 = await executor.ExecuteAsync(new CreationOperation { IdToCreate = "1234" });

        // Assert
        var evt = result1.ShouldBeContent<CreatedResourceEvent>();
        evt.Data.Links["self"].Href.Should().EndWith("/resources/1234");
    }

    [Test]
    public async Task When_ResourceCreated_Then_Attributes_Populated()
    {
        using var t = SystemTime.PauseForThread();

        // Arrange
        var executor = TestApiOperationExecutor.CreateHttp(o => o
            .WithOperation<CreationOperation>()
            .WithOperation<SelfQuery>()
            .AddResourceEvents<NullResourceEventRepository>());

        // Act
        var context = executor.HttpContextFor(new CreationOperation { IdToCreate = "1234" });

        var result = await executor.ExecuteAsync(context);

        // Assert
        var @event = result.ShouldBeContent<CreatedResourceEvent>();

        @event.Created.UtcDateTime.Should().BeCloseTo(t.UtcNow);
        @event.Object.Should().Be("event");
        @event.EventId.Should().Be("awesome.created");
        @event.ChangeType.Should().Be(ResourceEventChangeType.Created);
        @event.ResourceObject.Should().Be("awesome");
        @event.Href.Should().Be("https://api.blueprint-testing.com/api/resources/1234");
    }

    [Test]
    public async Task When_ResourceDeleted_Then_Attributes_Populated()
    {
        using var t = SystemTime.PauseForThread();

        // Arrange
        var executor = TestApiOperationExecutor.CreateHttp(o => o
            .WithOperation<DeleteOperation>()
            .WithOperation<SelfQuery>()
            .AddResourceEvents<NullResourceEventRepository>());

        // Act
        var context = executor.HttpContextFor(new DeleteOperation { IdToCreate = "1234" });

        var result = await executor.ExecuteAsync(context);

        // Assert
        var @event = result.ShouldBeContent<ResourceDeleted<AwesomeApiResource>>();

        @event.Created.UtcDateTime.Should().BeCloseTo(t.UtcNow);
        @event.Object.Should().Be("event");
        @event.EventId.Should().Be("awesome.deleted");
        @event.ChangeType.Should().Be(ResourceEventChangeType.Deleted);
        @event.ResourceObject.Should().Be("awesome");
        @event.Href.Should().Be("https://api.blueprint-testing.com/api/resources/1234");
    }

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

    [RootLink("/delete-a-resource")]
    public class DeleteOperation : ICommand
    {
        [Required]
        public string IdToCreate { get; set; }

        public ResourceDeleted<AwesomeApiResource> Invoke()
        {
            return new ResourceDeleted<AwesomeApiResource>(new SelfQuery { Id = IdToCreate });
        }
    }

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