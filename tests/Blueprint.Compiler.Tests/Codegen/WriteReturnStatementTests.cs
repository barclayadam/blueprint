using System.Linq;
using System.Threading.Tasks;
using Blueprint.Compiler.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Compiler.Tests.Codegen
{
    public class WriteReturnStatementTests
    {
        private GeneratedMethod theMethod;
        private SourceWriter theWriter;
        private Variable aVariable;

        private AsyncMode IfTheAsyncMode
        {
            set => theMethod.AsyncMode = value;
        }

        [SetUp]
        public void SetUp()
        {
            theMethod = GeneratedMethod.ForNoArg(Builder.NewType(), "Foo");
            theWriter = new SourceWriter();
            aVariable = new Variable(typeof(string), "name");
        }

        [Test]
        public void write_for_async_task_method()
        {
            IfTheAsyncMode = AsyncMode.AsyncTask;

            theWriter.Return(theMethod);

            theWriter.Code().ReadLines().Single()
                .Should().Be("return;");
        }

        [Test]
        public void write_for_return_from_last_node()
        {
            var expected = $"return {typeof(Task).FullName}.{nameof(Task.CompletedTask)};";

            IfTheAsyncMode = AsyncMode.ReturnFromLastNode;

            theWriter.Return(theMethod);

            theWriter.Code().ReadLines().Single()
                .Should().Be(expected);
        }


        [Test]
        public void write_for_variable_and_async_task_method()
        {
            IfTheAsyncMode = AsyncMode.AsyncTask;

            theWriter.Return(theMethod, aVariable);

            theWriter.Code().ReadLines().Single()
                .Should().Be("return name;");
        }

        [Test]
        public void write_for_variable_return_from_last_node()
        {
            var expected = $"return {typeof(Task).FullName}.{nameof(Task.FromResult)}(name);";

            IfTheAsyncMode = AsyncMode.ReturnFromLastNode;

            theWriter.Return(theMethod, aVariable);

            theWriter.Code().ReadLines().Single()
                .Should().Be(expected);
        }
    }
}
