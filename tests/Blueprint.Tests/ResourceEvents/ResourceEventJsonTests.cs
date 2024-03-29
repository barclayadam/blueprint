using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Blueprint.Http;
using FluentAssertions;
using NUnit.Framework;
using VerifyNUnit;

namespace Blueprint.Tests.ResourceEvents;

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
        rehydrated.Should().BeEquivalentTo(resourceEvent, o => o.Excluding(e => e.ResourceType));
    }

    [Test]
    public async Task Can_serialise_to_JSON()
    {
        // Arrange
        using var t = SystemTime.PauseForThread();
        t.SetUtcNow(new DateTime(2021, 07, 07, 0, 0, 0, DateTimeKind.Utc));

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

            Href = "https://www.my-api.com/my-resource/test-1234"
        };

        var prettyPrintOptions = new JsonSerializerOptions(JsonSerializerOptions) { WriteIndented = true };

        // Act
        var asJson = JsonSerializer.Serialize(resourceEvent, prettyPrintOptions);

        // Assert
        await Verifier.Verify(asJson);
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
        rehydrated.Should().BeEquivalentTo(resourceEvent);
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
        rehydrated.Should().BeEquivalentTo(resourceEvent);
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
        rehydrated.Should().BeEquivalentTo(resourceEvent);
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