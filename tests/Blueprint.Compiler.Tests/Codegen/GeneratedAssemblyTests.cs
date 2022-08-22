using System;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Compiler.Tests.Codegen;

public class GeneratedAssemblyTests
{
    [Test]
    public void same_type_added_twice_throws()
    {
        var assembly = Builder.Assembly();

        Action add = () => assembly.AddType("my.namespace", "AType", typeof(object));

        add();

        add.Should().ThrowExactly<ArgumentException>()
            .WithMessage($"A type already exists at my.namespace.AType");
    }

    [Test]
    public void same_type_different_namespace_successfully_creates_GeneratedType()
    {
        var assembly = Builder.Assembly();

        var type1 = assembly.AddType("my.namespace", "AType", typeof(object));
        var type2 = assembly.AddType("my.other.namespace", "AType", typeof(object));

        type1.Namespace.Should().Be("my.namespace");
        type1.TypeName.Should().Be("AType");

        type2.Namespace.Should().Be("my.other.namespace");
        type2.TypeName.Should().Be("AType");
    }
}