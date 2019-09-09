using System;
using System.Collections.Generic;

using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    public class ReturnFrame : SyncFrame
    {
        public Type ReturnType { get; }

        public ReturnFrame()
        {
        }

        public ReturnFrame(Type returnType)
        {
            ReturnType = returnType;
        }

        public ReturnFrame(Variable returnVariable)
        {
            ReturnedVariable = returnVariable;
        }

        public Variable ReturnedVariable { get; private set; }


        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            var code = ReturnedVariable == null ? "return;" : $"return {ReturnedVariable};";
            writer.Write(code);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            if (ReturnedVariable == null && ReturnType != null)
            {
                ReturnedVariable = chain.FindVariable(ReturnType);
            }
            
            if (ReturnedVariable != null)
            {
                yield return ReturnedVariable;
            }
        }
    }
}
