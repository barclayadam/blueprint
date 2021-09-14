using System.Collections.Generic;
using System.Text.Json;
using Blueprint.Http;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.ResourceEvents
{
    public class ResourceEventJsonTests
    {
        private static JsonSerializerOptions JsonSerializerOptions => BlueprintJsonOptions.DefaultSerializerOptions;

        [Test]
        public void Can_roundtrip_base_generic_instance()
        {
            // Arrange
            var resourceEvent = new ResourceEvent<MyResource>(
                ResourceEventChangeType.Created,
                "createdInTest",
                new MyResource("test-1234", "My named resource"))
            {
                Metadata = new Dictionary<string, object>
                {
                    ["stringKey"] = "value1",
                    ["numberKey"] = 1,
                    ["booleanKey"] = true,
                },

                SecureData = new Dictionary<string, object>
                {
                    ["key1"] = "value1",
                },

                ChangedValues = new Dictionary<string, object>
                {
                    ["key1"] = "changed-value1",
                },

                CorrelationId = "my-correlation-id",
            };

            // Act
            var asJson = JsonSerializer.Serialize(resourceEvent, JsonSerializerOptions);
            var rehydrated = JsonSerializer.Deserialize<ResourceEvent<MyResource>>(asJson, JsonSerializerOptions);

            // Assert
            rehydrated.Should().BeEquivalentTo(
                resourceEvent,
                o => o.Excluding(m => m.ResourceType));
        }

        [Test]
        public void Can_roundtrip_ResourceCreated_instance()
        {
            // Arrange
            var resourceEvent = new ResourceCreated<MyResource>(
                "createdInTest",
                new MyResource("test-1234", "My named resource"));

            // Act
            var asJson = JsonSerializer.Serialize(resourceEvent, JsonSerializerOptions);
            var rehydrated = JsonSerializer.Deserialize<ResourceCreated<MyResource>>(asJson, JsonSerializerOptions);

            // Assert
            rehydrated.Should().BeEquivalentTo(
                resourceEvent,
                o => o.Excluding(m => m.ResourceType));
        }

        [Test]
        public void Can_roundtrip_ResourceUpdated_instance()
        {
            // Arrange
            var resourceEvent = new ResourceUpdated<MyResource>(
                "madeAwesome",
                new MyResource("test-1234", "My named resource"));

            // Act
            var asJson = JsonSerializer.Serialize(resourceEvent, JsonSerializerOptions);
            var rehydrated = JsonSerializer.Deserialize<ResourceUpdated<MyResource>>(asJson, JsonSerializerOptions);

            // Assert
            rehydrated.Should().BeEquivalentTo(
                resourceEvent,
                o => o.Excluding(m => m.ResourceType));
        }

        [Test]
        public void Can_roundtrip_ResourceDeleted_instance()
        {
            // Arrange
            var resourceEvent = new ResourceDeleted<MyResource>(
                "obliterated",
                new MyResource("test-1234", "My named resource"));

            // Act
            var asJson = JsonSerializer.Serialize(resourceEvent, JsonSerializerOptions);
            var rehydrated = JsonSerializer.Deserialize<ResourceDeleted<MyResource>>(asJson, JsonSerializerOptions);

            // Assert
            rehydrated.Should().BeEquivalentTo(
                resourceEvent,
                o => o.Excluding(m => m.ResourceType));
        }

        private class MyResource : ApiResource
        {
            // For JSON
            private MyResource()
            {
                this.Id = default!;
                this.Name = default!;
            }

            public MyResource(string id, string name)
            {
                this.Id = id;
                this.Name = name;
            }

            public string Id { get; set; }

            public string Name { get; private set; }
        }
    }
}
