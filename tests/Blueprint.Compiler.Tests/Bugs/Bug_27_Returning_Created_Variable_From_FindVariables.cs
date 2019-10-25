using System.Collections.Generic;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Tests.Codegen;
using NUnit.Framework;

namespace Blueprint.Compiler.Tests.Bugs
{
    public class Bug_27_Returning_Created_Variable_From_FindVariables
    {
        [Test]
        public void do_not_do_a_stackoverflow_here()
        {
            var assembly = Builder.Assembly();
            var type = assembly.AddType("MyGuy", typeof(IHandler));
            var method = type.MethodFor("Go");

            method.Frames.Add(new CustomFrame());
            method.Frames.Add(new CustomFrame());
            method.Frames.Add(new CustomFrame());

            assembly.CompileAll();
        }

        public class CustomFrame : SyncFrame
        {
            public CustomFrame()
            {
                Variable = new Variable(typeof(bool), this);
            }

            public Variable Variable { get; }

            public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
            {
                // nothing
            }

            public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
            {
                yield return Variable;
            }
        }

        public interface IHandler
        {
            void Go();
        }
    }
}
