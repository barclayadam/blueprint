using System.Linq;
using System.Threading.Tasks;
using Blueprint.Compiler.Model;
using NUnit.Framework;
using Shouldly;

namespace Blueprint.Compiler.Tests.Codegen
{
    public class WriteReturnStatementTests
    {
        private GeneratedMethod theMethod;
        private SourceWriter theWriter;
        private Variable aVariable;

        private AsyncMode ifTheAsyncMode
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
            ifTheAsyncMode = AsyncMode.AsyncTask;

            theWriter.WriteReturnStatement(theMethod);

            theWriter.Code().ReadLines().Single()
                .ShouldBe("return;");
        }

        [Test]
        public void write_for_return_task()
        {
            var expected = $"return {typeof(Task).FullName}.{nameof(Task.CompletedTask)};";

            ifTheAsyncMode = AsyncMode.ReturnCompletedTask;

            theWriter.WriteReturnStatement(theMethod);

            theWriter.Code().ReadLines().Single()
                .ShouldBe(expected);
        }

        [Test]
        public void write_for_return_from_last_node()
        {
            var expected = $"return {typeof(Task).FullName}.{nameof(Task.CompletedTask)};";

            ifTheAsyncMode = AsyncMode.ReturnFromLastNode;

            theWriter.WriteReturnStatement(theMethod);

            theWriter.Code().ReadLines().Single()
                .ShouldBe(expected);
        }


        [Test]
        public void write_for_variable_and_async_task_method()
        {
            ifTheAsyncMode = AsyncMode.AsyncTask;

            theWriter.WriteReturnStatement(theMethod, aVariable);

            theWriter.Code().ReadLines().Single()
                .ShouldBe("return name;");
        }

        [Test]
        public void write_for_variable_and_return_task()
        {
            var expected = $"return {typeof(Task).FullName}.{nameof(Task.FromResult)}(name);";

            ifTheAsyncMode = AsyncMode.ReturnCompletedTask;

            theWriter.WriteReturnStatement(theMethod, aVariable);

            theWriter.Code().ReadLines().Single()
                .ShouldBe(expected);
        }

        [Test]
        public void write_for_variable_return_from_last_node()
        {
            var expected = $"return {typeof(Task).FullName}.{nameof(Task.FromResult)}(name);";

            ifTheAsyncMode = AsyncMode.ReturnFromLastNode;

            theWriter.WriteReturnStatement(theMethod, aVariable);

            theWriter.Code().ReadLines().Single()
                .ShouldBe(expected);
        }
    }


}
