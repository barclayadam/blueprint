using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Compiler.Tests.Codegen
{
    public class when_building_a_method_call_for_a_task_of_tuple
    {
        private readonly MethodCall theCall= MethodCall.For<MethodTarget>(x => x.AsyncReturnTuple());


        [Test]
        public void return_variable_usage()
        {
            theCall.ReturnVariable.Usage.Should().Be("(var red, var blue, var green)");
        }

        [Test]
        public void creates_does_not_contain_the_return_variable()
        {
            theCall.Creates.Should().NotContain(theCall.ReturnVariable);
        }

        [Test]
        public void has_creation_variables_for_the_tuple_types()
        {
            theCall.Creates.Should().BeSubsetOf(new [] { Variable.For<Red>(), Variable.For<Blue>(), Variable.For<Green>() });
        }
    }
}
