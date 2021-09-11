using System.Threading.Tasks;
using Blueprint.Http;
using Blueprint.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Blueprint.Tests.ResourceEvents
{
    public class Given_ResourceEvent_Data_Return
    {
        [Test]
        public async Task When_ApiResource_Returned_With_SelfQuery_Registered_Then_Href_Populated()
        {
            // Arrange
            var executor = TestApiOperationExecutor
                .CreateStandalone(o => o
                    .Http()
                    .AddHateoasLinks()
                    .AddResourceEvents<NullResourceEventRepository>()
                    .WithOperation<ResourceSelfOperation>()
                    .WithOperation<ResourceCreationOperation>()
                    .WithOperation<ResourceLinkWithoutIdOperation>()
                    .WithOperation<ResourceLinkWithIdOperation>()
                );

            // Act
            var result = await executor.ExecuteAsync<ResourceCreationOperation>();

            // Assert
            var okResult = result.ShouldBeOperationResultType<OkResult>();
            var content = okResult.Content.Should().BeOfType<ResourceCreated<ResourceToReturn>>().Subject;

            content.Href.Should().EndWith($"/resources/{ResourceCreationOperation.IdToCreate}");
        }

        [Test]
        public async Task When_ApiResource_Returned_Then_SelfLink_Populated()
        {
            // Arrange
            var executor = TestApiOperationExecutor
                .CreateStandalone(o => o
                    .Http()
                    .AddHateoasLinks()
                    .AddResourceEvents<NullResourceEventRepository>()
                    .WithOperation<ResourceSelfOperation>()
                    .WithOperation<ResourceCreationOperation>()
                    .WithOperation<ResourceLinkWithoutIdOperation>()
                    .WithOperation<ResourceLinkWithIdOperation>()
                );

            // Act
            var result = await executor.ExecuteAsync<ResourceCreationOperation>();

            // Assert
            var okResult = result.ShouldBeOperationResultType<OkResult>();
            var content = okResult.Content.Should().BeOfType<ResourceCreated<ResourceToReturn>>().Subject;

            // 2 + self
            content.Data.Links.Should().HaveCount(3);
        }

        public class ResourceToReturn : ApiResource
        {
            public string Id { get; set; }
        }

        [SelfLink(typeof(ResourceToReturn), "/resources/{Id}")]
        public class ResourceSelfOperation : ICommand<ResourceToReturn>
        {
            public string Id { get; set; }

            public ResourceToReturn Invoke()
            {
                return new ResourceToReturn { Id = this.Id, };
            }
        }

        [RootLink("/resources/create")]
        public class ResourceCreationOperation : ICommand<ResourceEvent<ResourceToReturn>>
        {
            public static readonly string IdToCreate = "12345";

            public ResourceEvent<ResourceToReturn> Invoke()
            {
                return new ResourceCreated<ResourceToReturn>(new ResourceToReturn { Id = IdToCreate, });
            }
        }

        [Link(typeof(ResourceToReturn), "/resources/child-link")]
        public class ResourceLinkWithoutIdOperation
        {
            public StatusCodeResult.OK Invoke()
            {
                return StatusCodeResult.OK.Instance;
            }
        }

        [Link(typeof(ResourceToReturn), "/resources/{Id}/child-link")]
        public class ResourceLinkWithIdOperation
        {
            public string Id { get; set; }

            public object Invoke()
            {
                return this.Id;
            }
        }
    }
}
