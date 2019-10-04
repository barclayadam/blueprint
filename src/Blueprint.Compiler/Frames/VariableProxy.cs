using System;
using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    public class VariableProxy
    {
        private readonly Type variableType;
        private readonly string name;
        private readonly string substitution;

        public VariableProxy(int index, Type variableType)
        {
            Index = index;
            this.variableType = variableType;

            substitution = $"~{index}~";
        }

        public VariableProxy(int index, Type variableType, string name)
        {
            Index = index;
            this.variableType = variableType;
            this.name = name;
        }

        public Variable Variable { get; private set; }

        public int Index { get; }

        public Variable Resolve(IMethodVariables variables)
        {
            Variable = string.IsNullOrEmpty(name)
                ? variables.FindVariable(variableType)
                : variables.FindVariableByName(variableType, name);

            return Variable;
        }

        public override string ToString()
        {
            return substitution;
        }

        public string Substitute(string code)
        {
            return code.Replace(substitution, Variable.Usage);
        }
    }
}
