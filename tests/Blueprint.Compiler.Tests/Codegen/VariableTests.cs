using System.Collections.Generic;
using Blueprint.Compiler.Model;
using NUnit.Framework;
using Shouldly;

namespace Blueprint.Compiler.Tests.Codegen
{
    public class VariableTests
    {
        [Test]
        public void override_the_name()
        {
            var variable = Variable.For<HyperdriveMotivator>();
            variable.OverrideName("thing");

            variable.Usage.ShouldBe("thing");
        }

        [Test]
        public void default_arg_name_of_normal_class()
        {
            Variable.DefaultArgName<HyperdriveMotivator>()
                .ShouldBe("hyperdriveMotivator");
        }

        [Test]
        public void default_arg_name_of_closed_interface()
        {
            Variable.DefaultArgName<IHyperdriveMotivator>()
                .ShouldBe("hyperdriveMotivator");
        }

        [Test]
        public void default_arg_name_of_array()
        {
            Variable.DefaultArgName<IWidget[]>()
                .ShouldBe("widgetArray");
        }

        [Test]
        public void default_arg_name_of_List()
        {
            Variable.DefaultArgName<IList<IWidget>>()
                .ShouldBe("widgetIList");

            Variable.DefaultArgName<List<IWidget>>()
                .ShouldBe("widgetList");

            Variable.DefaultArgName<IReadOnlyList<IWidget>>()
                .ShouldBe("widgetIReadOnlyList");
        }

        [Test]
        public void default_arg_name_of_enumerable()
        {
            Variable.DefaultArgName<IEnumerable<IWidget>>()
                .ShouldBe("widgetIEnumerable");
        }

        [Test]
        public void default_arg_name_of_generic_class_with_single_parameter()
        {
            Variable.DefaultArgName<FooHandler<HyperdriveMotivator>>()
                .ShouldBe("fooHandler");
        }

        [Test]
        public void default_arg_name_of_generic_interface_with_single_parameter()
        {
            Variable.DefaultArgName<IFooHandler<HyperdriveMotivator>>()
                .ShouldBe("fooHandler");
        }

        [Test]
        public void default_arg_name_of_open_generic_type()
        {
            Variable.DefaultArgName(typeof(IOpenGeneric<>))
                .ShouldBe("openGeneric");

            Variable.DefaultArgName(typeof(FooHandler<>)).ShouldBe("fooHandler");
        }

        [Test]
        public void default_arg_name_of_inner_class()
        {
            Variable.DefaultArgName<HyperdriveMotivator.InnerThing>()
                .ShouldBe("innerThing");
        }

        [Test]
        public void default_arg_name_of_inner_interface()
        {
            Variable.DefaultArgName<HyperdriveMotivator.IInnerThing>()
                .ShouldBe("innerThing");
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
