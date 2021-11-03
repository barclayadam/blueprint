using System;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler.Model
{
    public class CastVariable : Variable
    {
        public CastVariable(Variable parent, Type specificType)
            : base(specificType, $"(({specificType.FullNameInCode()}){parent})")
        {
            this.Dependencies.Add(parent);
            this.Inner = parent;
        }

        public Variable Inner { get; }
    }
}
