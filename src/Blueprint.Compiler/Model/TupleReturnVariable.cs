using System;
using System.Linq;

using Blueprint.Compiler.Util;

namespace Blueprint.Compiler.Model
{
    public class TupleReturnVariable : Variable
    {
        private readonly Variable[] _inner;

        public TupleReturnVariable(Type returnType, Variable[] inner) : base(returnType)
        {
            this._inner = inner;
        }

        public override string Usage => "(" + string.Join(", ", this._inner.Select(x => $"var {x.Usage}")) + ")";
    }
}
