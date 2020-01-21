using System.Linq;
using System.Threading.Tasks;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Compiler.Tests.Codegen
{
    public class MethodCallTester
    {
        [Test]
        public void determine_return_value_of_simple_type()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.GetValue());
            @call.ReturnVariable.Should().NotBeNull();

            @call.ReturnVariable.VariableType.Should().Be(typeof(string));
            @call.ReturnVariable.Usage.Should().Be("result_of_GetValue");
            @call.ReturnVariable.Creator.Should().BeSameAs(@call);
        }

        [Test]
        public void determine_return_value_of_not_simple_type()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.GetError());
            @call.ReturnVariable.Should().NotBeNull();

            @call.ReturnVariable.VariableType.Should().Be(typeof(ErrorMessage));
            @call.ReturnVariable.Usage.Should().Be(Variable.DefaultArgName(typeof(ErrorMessage)));
            @call.ReturnVariable.Creator.Should().BeSameAs(@call);
        }

        [Test]
        public void no_return_variable_on_void_method()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.Go(null));
            @call.ReturnVariable.Should().BeNull();
        }

        [Test]
        public void determine_return_value_of_Task_of_T_simple()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.GetString());
            @call.ReturnVariable.Should().NotBeNull();

            @call.ReturnVariable.VariableType.Should().Be(typeof(string));
            @call.ReturnVariable.Usage.Should().Be("result_of_GetString");
            @call.ReturnVariable.Creator.Should().BeSameAs(@call);
        }

        [Test]
        public void determine_return_value_of_not_simple_type_in_a_task()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.GetAsyncError());
            @call.ReturnVariable.Should().NotBeNull();

            @call.ReturnVariable.VariableType.Should().Be(typeof(ErrorMessage));
            @call.ReturnVariable.Usage.Should().Be(Variable.DefaultArgName(typeof(ErrorMessage)));
            @call.ReturnVariable.Creator.Should().BeSameAs(@call);
        }

        [Test]
        public void explicitly_set_parameter_by_variable_type()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.DoSomething(0, 0, null));

            var stringVariable = Variable.For<string>();
            var generalInt = Variable.For<int>();

            // Only one of that type, so it works
            @call.TrySetArgument(stringVariable)
                .Should().BeTrue();

            @call.Arguments[2].Should().BeSameAs(stringVariable);

            // Multiple parameters of the same type, nothing
            @call.TrySetArgument(generalInt).Should().BeFalse();
            @call.Arguments[0].Should().BeNull();
            @call.Arguments[1].Should().BeNull();
        }

        [Test]
        public void explicitly_set_parameter_by_variable_type_and_name()
        {
            var @call = MethodCall.For<MethodCallTarget>(x => x.DoSomething(0, 0, null));

            var generalInt = Variable.For<int>();

            @call.TrySetArgument("count", generalInt)
                .Should().BeTrue();

            @call.Arguments[0].Should().BeNull();
            @call.Arguments[1].Should().BeSameAs(generalInt);
            @call.Arguments[2].Should().BeNull();
        }

        [Test]
        public void default_disposal_mode_is_using()
        {
            MethodCall.For<MethodCallTarget>(x => x.Throw(null))
                .DisposalMode.Should().Be(DisposalMode.UsingBlock);
        }

        [Test]
        public void use_with_output_arguments_and_no_return_value()
        {
            var @call = new MethodCall(typeof(MethodCallTarget), nameof(MethodCallTarget.WithOuts));

            @call.ReturnVariable.Should().BeNull();

            @call.Arguments[0].Should().BeNull();
            @call.Arguments[1].Should().BeOfType<OutArgument>();
            @call.Arguments[2].Should().BeOfType<OutArgument>();

            @call.Creates.Select(x => x.VariableType)
                .Should().BeSubsetOf(new [] { typeof(string), typeof(int) });
        }


        [Test]
        public void use_with_output_arguments_and_return_value()
        {
            var @call = new MethodCall(typeof(MethodCallTarget), nameof(MethodCallTarget.ReturnAndOuts));

            @call.ReturnVariable.VariableType.Should().Be(typeof(bool));

            @call.Creates.Select(x => x.VariableType)
                .Should().BeSubsetOf(new [] { typeof(bool), typeof(string), typeof(int) });
        }
    }

    public class Ball
    {
    }

    public class Basketball : Ball
    {
    }

    public class MethodCallTarget
    {
        public void Throw(Ball ball)
        {
        }

        public void WithOuts(string in1, out string out1, out int out2)
        {
            out1 = "foo";
            out2 = 2;
        }

        public bool ReturnAndOuts(string in1, out string out1, out int out2)
        {
            out1 = "foo";
            out2 = 2;

            return true;
        }

        public string GetValue()
        {
            return "foo";
        }

        public static void GoStatic()
        {
        }

        public ErrorMessage GetError()
        {
            return null;
        }

        public Task<ErrorMessage> GetAsyncError()
        {
            return null;
        }

        public void Go(string text)
        {
        }

        public void DoSomething(int age, int count, string name)
        {
        }

        public Task<string> GetString()
        {
            return Task.FromResult("foo");
        }
    }

    public class ErrorMessage {}
}
