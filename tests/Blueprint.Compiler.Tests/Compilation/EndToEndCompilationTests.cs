using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Tests.Codegen;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Shouldly;

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
                .Generate(3, 4).ShouldBe(7);

            adder.SourceCode.ShouldContain("public class Adder : Blueprint.Compiler.Tests.Compilation.INumberGenerator");
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
                .Generate(3, 4).ShouldBe(12);

            multiplier.SourceCode.ShouldContain("public class Multiplier : Blueprint.Compiler.Tests.Compilation.INumberGenerator");
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
                .Generate(3, 4).ShouldBe(12);

            multiplier.SourceCode.ShouldContain("public class Multiplier : Blueprint.Compiler.Tests.Compilation.NumberGenerator");
        }

        [Test]
        public void frame_with_syntax_error()
        {
            var assembly = Builder.Assembly();

            var multiplier = assembly.AddType("Multiplier", typeof(NumberGenerator));
            multiplier.MethodFor(nameof(NumberGenerator.Generate)).Frames.Append<SyntaxErrorFrame>();

            Action tryCompile = () => assembly.CompileAll();

            var compilationException = tryCompile.ShouldThrow<CompilationException>();

            compilationException.Failures.ShouldNotBeEmpty();

            var compileFailure = compilationException.Failures.Single();
            compileFailure.Id.ShouldNotBeEmpty("CS0103");
            compileFailure.Severity.ShouldBe(DiagnosticSeverity.Error);

            compilationException.Message.ShouldContainIgnoringNewlines(@"Multiplier.cs(11,26): error CS0103: The name 'oopstwo' does not exist in the current context
        {

            return one + oopstwo;

                         ^^^^^^^
        }");

            compilationException.Code.ShouldContainIgnoringNewlines(@"using System.Threading.Tasks;

namespace Blueprint.Compiler.Tests
{
    public class Multiplier : Blueprint.Compiler.Tests.Compilation.NumberGenerator
    {


        public override int Generate(int one, int two)
        {
            return one + oopstwo;
        }

    }

}");
        }
    }

    public class SyntaxErrorFrame : SyncFrame
    {
        private Variable one;
        private Variable two;

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"return {one} + oops{two};");
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            yield return one = chain.FindVariableByName(typeof(int), "one");
            yield return two = chain.FindVariableByName(typeof(int), "two");
        }
    }

    public class AddFrame : SyncFrame
    {
        private Variable one;
        private Variable two;

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"return {one} + {two};");
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            yield return one = chain.FindVariableByName(typeof(int), "one");
            yield return two = chain.FindVariableByName(typeof(int), "two");
        }
    }

    public class MultiplyFrame : SyncFrame
    {
        private Variable one;
        private Variable two;

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteLine($"return {one} * {two};");
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            yield return one = chain.FindVariableByName(typeof(int), "one");
            yield return two = chain.FindVariableByName(typeof(int), "two");
        }
    }

    public interface INumberGenerator
    {
        int Generate(int one, int two);
    }

    public abstract class NumberGenerator
    {
        public abstract int Generate(int one, int two);
    }
}
