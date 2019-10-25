using System.Collections.Generic;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Tests.Scenarios;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Compiler.Tests.Codegen.Scenarios
{
    public class basic_execution
    {
        [Test]
        public void simple_execution_with_action()
        {
            var result = CodegenScenario.ForAction<Tracer>((t, m) => m.Frames.Call<Tracer>(x => x.Call()));

            var tracer = new Tracer();
            result.Object.DoStuff(tracer);

            tracer.Called.Should().BeTrue();

            result.LinesOfCode.Should().Contain("arg1.Call();");
        }

        [Test]
        public void simple_execution_with_action_2()
        {
            var result = CodegenScenario.ForAction<Tracer>(m => m.Frames.Call<Tracer>(x => x.Call()));

            var tracer = new Tracer();
            result.Object.DoStuff(tracer);

            tracer.Called.Should().BeTrue();

            result.LinesOfCode.Should().Contain("arg1.Call();");
        }

        [Test]
        public void simple_execution_with_one_input_and_output()
        {
            var result = CodegenScenario.ForBuilds<int, int>((t, m) => m.Frames.Append<AddTwoFrame>());

            result.Object.Create(5).Should().Be(7);
        }

        [Test]
        public void simple_execution_with_one_input_and_output_2()
        {
            var result = CodegenScenario.ForBuilds<int, int>(m => m.Frames.Append<AddTwoFrame>());

            result.Object.Create(5).Should().Be(7);
        }
    }


    public class Tracer
    {
        public void Call()
        {
            Called = true;
        }

        public bool Called { get; set; }
    }



    public class AddTwoFrame : SyncFrame
    {
        private Variable _number;

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"return {_number.Usage} + 2;");
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            _number = chain.FindVariable(typeof(int));
            yield return _number;
        }
    }
}
