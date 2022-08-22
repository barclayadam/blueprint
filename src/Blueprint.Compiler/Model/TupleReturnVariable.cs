using System;
using System.Linq;

namespace Blueprint.Compiler.Model;

public class TupleReturnVariable : Variable
{
    public TupleReturnVariable(Type returnType, Variable[] inner)
        : base(returnType, "(" + string.Join(", ", inner.Select(x => $"var {x.Usage}")) + ")")
    {
    }
}