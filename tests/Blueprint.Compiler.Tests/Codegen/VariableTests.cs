using System.Collections.Generic;
using Blueprint.Compiler.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Compiler.Tests.Codegen
{
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
            Variable.DefaultArgName<HyperdriveMotivator>()
                .Should().Be("hyperdriveMotivator");
        }

        [Test]
        public void default_arg_name_of_closed_interface()
        {
            Variable.DefaultArgName<IHyperdriveMotivator>()
                .Should().Be("hyperdriveMotivator");
        }

        [Test]
        public void default_arg_name_of_array()
        {
            Variable.DefaultArgName<IWidget[]>()
                .Should().Be("widgetArray");
        }

        [Test]
        public void default_arg_name_of_List()
        {
            Variable.DefaultArgName<IList<IWidget>>()
                .Should().Be("widgetIList");

            Variable.DefaultArgName<List<IWidget>>()
                .Should().Be("widgetList");

            Variable.DefaultArgName<IReadOnlyList<IWidget>>()
                .Should().Be("widgetIReadOnlyList");
        }

        [Test]
        public void default_arg_name_of_enumerable()
        {
            Variable.DefaultArgName<IEnumerable<IWidget>>()
                .Should().Be("widgetIEnumerable");
        }

        [Test]
        public void default_arg_name_of_generic_class_with_single_parameter()
        {
            Variable.DefaultArgName<FooHandler<HyperdriveMotivator>>()
                .Should().Be("hyperdriveMotivatorFooHandler");
        }

        [Test]
        public void default_arg_name_of_generic_interface_with_single_parameter()
        {
            Variable.DefaultArgName<IFooHandler<HyperdriveMotivator>>()
                .Should().Be("hyperdriveMotivatorIFooHandler");
        }

        [Test]
        public void default_arg_name_of_open_generic_type()
        {
            Variable.DefaultArgName(typeof(IOpenGeneric<>))
                .Should().Be("openGeneric");

            Variable.DefaultArgName(typeof(FooHandler<>)).Should().Be("fooHandler");
        }

        [Test]
        public void default_arg_name_of_inner_class()
        {
            Variable.DefaultArgName<HyperdriveMotivator.InnerThing>()
                .Should().Be("innerThing");
        }

        [Test]
        public void default_arg_name_of_inner_interface()
        {
            Variable.DefaultArgName<HyperdriveMotivator.IInnerThing>()
                .Should().Be("innerThing");
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
}
