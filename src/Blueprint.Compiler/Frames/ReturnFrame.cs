using System;
using System.Collections.Generic;

using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    public class ReturnFrame : SyncFrame
    {
        private readonly Type returnType;

        public ReturnFrame()
        {
        }

        public ReturnFrame(Type returnType)
        {
            this.returnType = returnType;
        }

        public ReturnFrame(Variable returnVariable)
        {
            ReturnedVariable = returnVariable;
        }

        public Variable ReturnedVariable { get; private set; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write(ToString());
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            if (ReturnedVariable == null && returnType != null)
            {
                ReturnedVariable = chain.FindVariable(returnType);
            }

            if (ReturnedVariable != null)
            {
                yield return ReturnedVariable;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ReturnedVariable == null ? "return;" : $"return {ReturnedVariable};";
        }
    }
}
