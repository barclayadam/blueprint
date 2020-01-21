namespace Blueprint.Compiler.Tests.Codegen
{
    internal static class Builder
    {
        public static GeneratedType NewType(string @namespace = "Blueprint.Compiler.Tests", string typeName = "Foo")
        {
            return new GeneratedType(Assembly(@namespace), typeName);
        }

        public static GeneratedAssembly Assembly(string @namespace = "Blueprint.Compiler.Tests")
        {
            return new GeneratedAssembly(Rules(@namespace));
        }

        public static GenerationRules Rules(string @namespace = "Blueprint.Compiler.Tests")
        {
            return new GenerationRules(@namespace);
        }
    }
}
