namespace Blueprint.Compiler.Tests.Codegen;

internal static class Builder
{
    private static int scenarioCount = 1;

    public static GeneratedType NewType(string assemblyName = null, string typeName = "Foo")
    {
        return new GeneratedType(Assembly(assemblyName), typeName, assemblyName);
    }

    public static GeneratedAssembly Assembly(string assemblyName = null)
    {
        return new GeneratedAssembly(Rules(assemblyName));
    }

    public static GenerationRules Rules(string assemblyName = null)
    {
        return new GenerationRules
        {
            AssemblyName = assemblyName ?? "Blueprint.Compiler.Tests" + scenarioCount++,
        };
    }
}