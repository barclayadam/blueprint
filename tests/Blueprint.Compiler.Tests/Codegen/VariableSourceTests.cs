using System;
using System.Linq;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Blueprint.Compiler.Tests.Codegen
{
    public class VariableSourceTests
    {
        [Test]
        public void add_Frame_immediately_before_when_Source_creates_new_Frame()
        {
            var assembly = Builder.Assembly();
            var type = assembly.AddType("Tests", "MyGuy", typeof(IHandler));
            var method = type.MethodFor("Go");

            method.Sources.Add(new FrameGeneratingVariableSource());

            method.Frames.Add(new CommentFrame("Start of method"));
            method.Frames.Add(new CustomFrame());

            assembly.CompileAll();

            type.SourceCode.ReadLines().Select(l => l.Trim()).Should().ContainInOrder(
                "// Start of method",
                "// SourceFrame",
                "// CustomFrame");
        }

        public class FrameGeneratingVariableSource : IVariableSource
        {
            public Variable TryFindVariable(IMethodVariables variables, Type type)
            {
                var frameToInject = new SourceFrame();

                return frameToInject.Variable;
            }
        }

        public class CustomFrame : SyncFrame
        {
            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                var handlerFromSource = variables.FindVariable(typeof(IHandler));
                writer.Comment(nameof(CustomFrame));

                next();
            }
        }

        public class SourceFrame : SyncFrame
        {
            public SourceFrame()
            {
                Variable = new Variable(typeof(IHandler), this);
            }

            public Variable Variable { get; }

            protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
            {
                writer.Comment(nameof(SourceFrame));

                next();
            }
        }

        public interface IHandler
        {
            void Go();
        }
    }
}
