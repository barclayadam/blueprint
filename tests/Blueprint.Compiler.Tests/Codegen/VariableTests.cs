using System.Collections.Generic;
using Blueprint.Compiler.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Compiler.Tests.Codegen;

public class VariableTests
{
    [Test]
    public void override_the_name()
    {
        var variable = Variable.For<HyperdriveMotivator>();
        variable.OverrideName("thing");

        variable.Usage.Should().Be("thing");
    }

    [Test]
    public void default_arg_name_of_normal_class()
    {
        Variable.DefaultName<HyperdriveMotivator>()
            .Should().Be("hyperdriveMotivator");
    }

    [Test]
    public void default_arg_name_of_closed_interface()
    {
        Variable.DefaultName<IHyperdriveMotivator>()
            .Should().Be("hyperdriveMotivator");
    }

    [Test]
    public void default_arg_name_of_array()
    {
        Variable.DefaultName<IWidget[]>()
            .Should().Be("widgetArray");
    }

    [Test]
    public void default_arg_name_of_List()
    {
        Variable.DefaultName<IList<IWidget>>()
            .Should().Be("widgetIList");

        Variable.DefaultName<List<IWidget>>()
            .Should().Be("widgetList");

        Variable.DefaultName<IReadOnlyList<IWidget>>()
            .Should().Be("widgetIReadOnlyList");
    }

    [Test]
    public void default_arg_name_of_enumerable()
    {
        Variable.DefaultName<IEnumerable<IWidget>>()
            .Should().Be("widgetIEnumerable");
    }

    [Test]
    public void default_arg_name_of_generic_class_with_single_parameter()
    {
        Variable.DefaultName<FooHandler<HyperdriveMotivator>>()
            .Should().Be("hyperdriveMotivatorFooHandler");
    }

    [Test]
    public void default_arg_name_of_generic_interface_with_single_parameter()
    {
        Variable.DefaultName<IFooHandler<HyperdriveMotivator>>()
            .Should().Be("hyperdriveMotivatorIFooHandler");
    }

    [Test]
    public void default_arg_name_of_open_generic_type()
    {
        Variable.DefaultName(typeof(IOpenGeneric<>))
            .Should().Be("openGeneric");

        Variable.DefaultName(typeof(FooHandler<>)).Should().Be("fooHandler");
    }

    [Test]
    public void default_arg_name_of_inner_class()
    {
        Variable.DefaultName<HyperdriveMotivator.InnerThing>()
            .Should().Be("innerThing");
    }

    [Test]
    public void default_arg_name_of_inner_interface()
    {
        Variable.DefaultName<HyperdriveMotivator.IInnerThing>()
            .Should().Be("innerThing");
    }

    [Test]
    public void generic_variable_GetProperty()
    {
        Variable.For<string>("stringVariableName")
            .GetProperty(s => s.Length)
            .Usage
            .Should().Be("stringVariableName.Length");
    }
}

public interface IWidget{}

public class FooHandler<T>
{

}

public interface IOpenGeneric<T>{}

public interface IFooHandler<T>
{

}

public interface IHyperdriveMotivator
{

}

public class HyperdriveMotivator
{
    public class InnerThing
    {

    }

    public interface IInnerThing
    {

    }
}