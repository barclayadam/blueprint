using System.Linq;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Tests.Scenarios;
using NUnit.Framework;
using Shouldly;

namespace Blueprint.Compiler.Tests.Codegen
{
    public class ReturnFrameTests
    {
        [Test]
        public void simple_use_case_no_value()
        {
            var result = CodegenScenario.ForBaseOf<ISimpleAction>(m => m.Frames.Add(new ReturnFrame()));

            result.LinesOfCode.ShouldContain("return;");
        }

        [Test]
        public void return_a_variable_by_type()
        {
            var result = CodegenScenario.ForBuilds<int, int>(m => m.Frames.Return(typeof(int)));

            result.LinesOfCode.ShouldContain("return arg1;");
            result.Object.Create(5).ShouldBe(5);
        }

        [Test]
        public void return_explicit_variable()
        {
            var result = CodegenScenario.ForBuilds<int, int>(m =>
            {
                var arg = m.Arguments.Single();
                m.Frames.Return(arg);
            });

            result.LinesOfCode.ShouldContain("return arg1;");
            result.Object.Create(5).ShouldBe(5);
        }
    }

    public interface ISimpleAction
    {
        void Go();
    }
}
