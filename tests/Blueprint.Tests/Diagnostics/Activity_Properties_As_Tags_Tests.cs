using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Blueprint.Auditing;
using Blueprint.Testing;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Tests.Diagnostics;

public class Activity_Properties_As_Tags_Tests
{
    [Test]
    public async Task When_Executed_Then_Activity_Tags_Set()
    {
        // Arrange
        var expected = new SupportedTypesOperation
        {
            IntegerProperty = 761,
            EnumProperty = OperationEnum.EnumOne,
            GuidProperty = Guid.NewGuid(),
            StringProperty = "a string",
            NullableIntegerProperty = null,
        };

        var handler = new TestApiOperationHandler<SupportedTypesOperation>(null);
        var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithHandler(handler));

        using var activity = Activity.Current = new Activity("PopulationTest").Start();

        // Act
        await executor.ExecuteAsync(expected);

        // Assert
        var tags = activity.TagObjects.ToDictionary(k => k.Key, v => v.Value);
            
        tags.Should().Contain($"{nameof(SupportedTypesOperation)}.{nameof(expected.IntegerProperty)}", expected.IntegerProperty);
        tags.Should().Contain($"{nameof(SupportedTypesOperation)}.{nameof(expected.EnumProperty)}", expected.EnumProperty);
        tags.Should().Contain($"{nameof(SupportedTypesOperation)}.{nameof(expected.GuidProperty)}", expected.GuidProperty);
        tags.Should().Contain($"{nameof(SupportedTypesOperation)}.{nameof(expected.StringProperty)}", expected.StringProperty);
            
        // Nulls are ignored / removed
        tags.Should().NotContain($"{nameof(SupportedTypesOperation)}.{nameof(expected.NullableIntegerProperty)}", expected.NullableIntegerProperty);
    }
        
    [Test]
    public async Task When_Executed_Then_Unsupported_Types_Not_Set()
    {
        // Arrange
        var expected = new NonSupportedTypeOperation
        {
            ListProperty = new List<string>(),
            ChildComplexProperty = new SensitiveOperation(),
            StringArray = Array.Empty<string>(),
        };

        var handler = new TestApiOperationHandler<NonSupportedTypeOperation>(null);
        var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithHandler(handler));

        using var activity = Activity.Current = new Activity("PopulationTest").Start();

        // Act
        await executor.ExecuteAsync(expected);

        // Assert
        activity.TagObjects.Should().BeEmpty();
    }
        
    [Test]
    public async Task When_Executed_Then_Sensitive_Properties_Excluded_From_Activity_Tags()
    {
        // Arrange
        var expected = new SensitiveOperation
        {
            NotSensitiveProperty = "NotSensitiveProperty",
            Password = "Password",
            PasswordOne = "PasswordOne",
            ASensitiveProperty = "ASensitiveProperty",
            ADoNotAuditProperty = "ADoNotAuditProperty",
        };

        var handler = new TestApiOperationHandler<SensitiveOperation>(null);
        var executor = TestApiOperationExecutor.CreateStandalone(o => o.WithHandler(handler));

        using var activity = Activity.Current = new Activity("PopulationTest").Start();

        // Act
        await executor.ExecuteAsync(expected);

        // Assert
        var tags = activity.TagObjects.ToDictionary(k => k.Key, v => v.Value);
            
        tags.Should().HaveCount(1);
        tags.Should().Contain($"{nameof(SensitiveOperation)}.{nameof(expected.NotSensitiveProperty)}", expected.NotSensitiveProperty);
    }

    public enum OperationEnum
    {
        EnumOne,
        EnumTwo
    }

    [RootLink("/a-static-route")]
    public class SupportedTypesOperation
    {
        public int IntegerProperty { get; set; }

        public int? NullableIntegerProperty { get; set; }

        public string StringProperty { get; set; }

        public OperationEnum EnumProperty { get; set; }

        public Guid GuidProperty { get; set; }
    }
        

    [RootLink("/a-static-route")]
    public class NonSupportedTypeOperation
    {
        public List<string> ListProperty { get; set; }
            
        public SensitiveOperation ChildComplexProperty { get; set; }

        public string[] StringArray { get; set; }
    }
        
    [RootLink("/sensitive")]
    public class SensitiveOperation
    {
        public string NotSensitiveProperty { get; set; }
            
        public string PasswordOne { get; set; }
            
        public string Password { get; set; }
            
        [Sensitive]
        public string ASensitiveProperty { get; set; }
            
        [DoNotAudit]
        public string ADoNotAuditProperty { get; set; }
    }
}