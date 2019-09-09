using System;
using System.Linq;

using Blueprint.Compiler.Util;

namespace Blueprint.Compiler.Model
{
    public class ValueTypeReturnVariable : Variable
    {
        private readonly Variable[] inner;

        public ValueTypeReturnVariable(Type returnType, Variable[] inner) : base(returnType)
        {
            this.inner = inner;
        }

        public override string Usage => "(" + inner.Select(x => $"var {x.Usage}").Join(", ") + ")";
    }
}