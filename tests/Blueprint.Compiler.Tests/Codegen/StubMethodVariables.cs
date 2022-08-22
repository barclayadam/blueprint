using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Tests.Codegen;

public class StubMethodVariables : IMethodVariables
{
    public readonly Dictionary<Type, Variable> variables = new Dictionary<Type, Variable>();
    public readonly IList<Variable> extras = new List<Variable>();

    public Variable FindVariable(Type type)
    {
        return variables[type];
    }

    public Variable FindVariableByName(Type dependency, string name)
    {
        var found = TryFindVariableByName(dependency, name, out var variable);
        if (found) return variable;

        throw new Exception($"No known variable for {dependency} named {name}");
    }

    public bool TryFindVariableByName(Type dependency, string name, out Variable variable)
    {
        variable = variables.Values.Concat(extras).FirstOrDefault(x => x.Usage == name && x.VariableType == dependency);
        return variable != null;
    }

    public Variable TryFindVariable(Type type)
    {
        return variables.ContainsKey(type) ? variables[type] : null;
    }

    public void Store(Variable variable)
    {
        variables[variable.VariableType] = variable;
        extras.Add(variable);
    }

    public void Store<T>(string variableName = null)
    {
        var variable = Variable.For<T>(variableName);
        Store(variable);
    }
}