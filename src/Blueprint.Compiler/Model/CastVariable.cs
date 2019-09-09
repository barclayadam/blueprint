using System;

namespace Blueprint.Compiler.Model
{
    public class CastVariable : Variable
    {
        public CastVariable(Variable parent, Type specificType) : base(specificType,
            $"(({specificType.FullNameInCode()}){parent})")
        {
            Dependencies.Add(parent);
            Inner = parent;
        }

        // strictly for easier testing
        public Variable Inner { get; }
    }
}
