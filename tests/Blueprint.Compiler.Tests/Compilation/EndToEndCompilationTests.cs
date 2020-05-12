using System;
using System.Linq;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Tests.Codegen;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace Blueprint.Compiler.Tests.Compilation
{
    public class EndToEndCompilationTests
    {
        [Test]
        public void dynamic_types_no_fields_adder()
        {
            var assembly = Builder.Assembly();

            var adder = assembly.AddType("Adder", typeof(INumberGenerator));
            adder.MethodFor("Generate").Frames.Append<AddFrame>();

            assembly.CompileAll();

            Activator.CreateInstance(adder.CompiledType)
                .As<INumberGenerator>()
                .Generate(3).Should().Be(6);

            adder.SourceCode.Should().Contain("public class Adder : Blueprint.Compiler.Tests.Compilation.INumberGenerator");
        }

        [Test]
        public void dynamic_types_no_fields_multiplier()
        {
            var assembly = Builder.Assembly();

            var multiplier = assembly.AddType("Multiplier", typeof(INumberGenerator));
            multiplier.MethodFor(nameof(INumberGenerator.Generate)).Frames.Append<MultiplyFrame>();

            assembly.CompileAll();

            Activator.CreateInstance(multiplier.CompiledType)
                .As<INumberGenerator>()
                .Generate(3).Should().Be(9);

            multiplier.SourceCode.Should().Contain("public class Multiplier : Blueprint.Compiler.Tests.Compilation.INumberGenerator");
        }

        [Test]
        public void type_with_base_class()
        {
            var assembly = Builder.Assembly();

            var multiplier = assembly.AddType("Multiplier", typeof(NumberGenerator));
            multiplier.MethodFor(nameof(NumberGenerator.Generate)).Frames.Append<MultiplyFrame>();

            assembly.CompileAll();

            Activator.CreateInstance(multiplier.CompiledType)
                .As<NumberGenerator>()
                .Generate(3).Should().Be(9);

            multiplier.SourceCode.Should().Contain("public class Multiplier : Blueprint.Compiler.Tests.Compilation.NumberGenerator");
        }

        [Test]
        public void frame_with_syntax_error()
        {
            var assembly = Builder.Assembly();

            var multiplier = assembly.AddType("Multiplier", typeof(NumberGenerator));
            multiplier.MethodFor(nameof(NumberGenerator.Generate)).Frames.Append<SyntaxErrorFrame>();

            Action tryCompile = () => assembly.CompileAll();

            var compilationException = tryCompile.Should().ThrowExactly<CompilationException>().Subject.Single();

            compilationException.Failures.Should().NotBeEmpty();

            var compileFailure = compilationException.Failures.Single();
            compileFailure.Id.Should().Be("CS0103");
            compileFailure.Severity.Should().Be(DiagnosticSeverity.Error);

            compilationException.Message.ShouldContainIgnoringNewlines(@"Multiplier.cs(13,26): error CS0103: The name 'oopsone' does not exist in the current context
        {

            return one + oopsone;

                         ^^^^^^^
        }");

            compilationException.Code.ShouldContainIgnoringNewlines(@"using System.Threading.Tasks;

namespace Blueprint.Compiler.Tests
{
    public class Multiplier : Blueprint.Compiler.Tests.Compilation.NumberGenerator
    {


        public override int Generate(int one)
        {
            return one + oopsone;
        }

    }

}");
        }
    }

    public class SyntaxErrorFrame : SyncFrame
    {
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            var one = variables.FindVariable(typeof(int));

            writer.WriteLine($"return {one} + oops{one};");
        }
    }

    public class AddFrame : SyncFrame
    {
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            var one = variables.FindVariable(typeof(int));

            writer.WriteLine($"return {one} + {one};");
        }
    }

    public class MultiplyFrame : SyncFrame
    {
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            var one = variables.FindVariable(typeof(int));

            writer.WriteLine($"return {one} * {one};");
        }
    }

    public interface INumberGenerator
    {
        int Generate(int one);
    }

    public abstract class NumberGenerator
    {
        public abstract int Generate(int one);
    }
}
