using System;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Compiler.Tests.Codegen
{
    public class ReflectionExtensionsTests
    {
        [Test]
        public void get_full_name_in_code_for_generic_type()
        {
            typeof(Handler<Message1>).FullNameInCode()
                .Should().Be($"Blueprint.Compiler.Tests.Codegen.Handler<{typeof(Message1).FullName}>");
        }

        public interface ISomeInterface<T>
        {
        }

        [Test]
        public void get_full_name_in_code_for_inner_generic_type()
        {
            typeof(ISomeInterface<string>).FullNameInCode()
                .Should().Be("Blueprint.Compiler.Tests.Codegen.ReflectionExtensionsTests.ISomeInterface<string>");
        }

        [Test]
        public void get_name_in_code_for_inner_generic_type()
        {
            typeof(ISomeInterface<string>).NameInCode()
                .Should().Be("ReflectionExtensionsTests.ISomeInterface<string>");
        }

        // SAMPLE: get-the-type-name-in-code
        [Theory]
        [TestCase(typeof(void), "void")]
        [TestCase(typeof(int), "int")]
        [TestCase(typeof(string), "string")]
        [TestCase(typeof(long), "long")]
        [TestCase(typeof(bool), "bool")]
        [TestCase(typeof(double), "double")]
        [TestCase(typeof(object), "object")]
        [TestCase(typeof(Message1), "Message1")]
        [TestCase(typeof(Handler<Message1>), "Handler<Blueprint.Compiler.Tests.Codegen.Message1>")]
        [TestCase(typeof(Handler<string>), "Handler<string>")]
        public void alias_name_of_task(Type type, string name)
        {
            // Gets the type name
            type.NameInCode().Should().Be(name);
        }
        // ENDSAMPLE

        // SAMPLE: get-the-full-type-name-in-code
        [Theory]
        [TestCase(typeof(void), "void")]
        [TestCase(typeof(int), "int")]
        [TestCase(typeof(string), "string")]
        [TestCase(typeof(long), "long")]
        [TestCase(typeof(bool), "bool")]
        [TestCase(typeof(double), "double")]
        [TestCase(typeof(object), "object")]
        [TestCase(typeof(Message1), "Blueprint.Compiler.Tests.Codegen.Message1")]
        [TestCase(typeof(Handler<Message1>), "Blueprint.Compiler.Tests.Codegen.Handler<Blueprint.Compiler.Tests.Codegen.Message1>")]
        [TestCase(typeof(Handler<string>), "Blueprint.Compiler.Tests.Codegen.Handler<string>")]
        public void alias_full_name_of_task(Type type, string name)
        {
            type.FullNameInCode().Should().Be(name);
        }
        // ENDSAMPLE

        [Test]
        public void name_in_code_of_inner_type()
        {
            typeof(ThingHolder.Thing1).NameInCode().Should().Be("ThingHolder.Thing1");
        }

        [Test]
        public void full_name_in_code_of_generic_types_nested_type()
        {
            typeof(GenericTestClassWithNested<string>.NestedTestClass).FullNameInCode().Should().Be("Blueprint.Compiler.Tests.Codegen.GenericTestClassWithNested<string>.NestedTestClass");
        }
    }

    public class ThingHolder
    {
        public class Thing1
        {
        }
    }

    public class Handler<T>
    {
    }

    public class Message1{}

    public class GenericTestClassWithNested<T>
    {
        public sealed class NestedTestClass
        {
        }
    }
}
