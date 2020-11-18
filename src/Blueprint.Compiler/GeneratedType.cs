using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler
{
    public class GeneratedType : IVariableSource
    {
        private readonly IList<Type> interfaces = new List<Type>();
        private readonly IList<GeneratedMethod> methods = new List<GeneratedMethod>();

        internal GeneratedType(GeneratedAssembly generatedAssembly, string typeName, string @namespace)
        {
            GeneratedAssembly = generatedAssembly;

            Namespace = @namespace;
            TypeName = typeName;
        }

        /// <summary>
        /// Gets the generated assembly this type belongs to.
        /// </summary>
        public GeneratedAssembly GeneratedAssembly { get; }

        public IList<Setter> Setters { get; } = new List<Setter>();

        public string Namespace { get; set; }

        public string TypeName { get; }

        public Type BaseType { get; private set; }

        public InjectedField[] BaseConstructorArguments { get; private set; } = new InjectedField[0];

        public HashSet<string> Namespaces { get; } = new HashSet<string>();

        public List<InjectedField> AllInjectedFields { get; } = new List<InjectedField>();

        public List<StaticField> AllStaticFields { get; } = new List<StaticField>();

        public IEnumerable<Type> Interfaces => interfaces;

        public IEnumerable<GeneratedMethod> Methods => methods;

        public string SourceCode { get; set; }

        public Type CompiledType { get; private set; }

        public GeneratedType InheritsFrom<T>()
        {
            return InheritsFrom(typeof(T));
        }

        public GeneratedType InheritsFrom(Type baseType)
        {
            var ctors = baseType.GetConstructors();

            if (ctors.Length > 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(baseType),
                    $"The base type for the code generation must only have one public constructor. {baseType.FullNameInCode()} has {ctors.Length}");
            }

            if (ctors.Length == 1)
            {
                BaseConstructorArguments = ctors.Single().GetParameters()
                    .Select(x => new InjectedField(x.ParameterType, x.Name)).ToArray();

                AllInjectedFields.AddRange(BaseConstructorArguments);
            }

            BaseType = baseType;

            foreach (var methodInfo in baseType.GetMethods().Where(x => x.DeclaringType != typeof(object)).Where(x => x.CanBeOverridden()))
            {
                methods.Add(new GeneratedMethod(this, methodInfo)
                {
                    Overrides = true,
                });
            }

            return this;
        }

        public GeneratedType Implements(Type type)
        {
            if (!type.GetTypeInfo().IsInterface)
            {
                throw new ArgumentOutOfRangeException(nameof(type), "Must be an interface type");
            }

            interfaces.Add(type);

            foreach (var methodInfo in type.GetMethods().Where(x => x.DeclaringType != typeof(object)))
            {
                methods.Add(new GeneratedMethod(this, methodInfo));
            }

            return this;
        }

        public GeneratedType Implements<T>()
        {
            return Implements(typeof(T));
        }

        public void AddMethod(GeneratedMethod method)
        {
            methods.Add(method);
        }

        public GeneratedMethod MethodFor(string methodName)
        {
            return methods.FirstOrDefault(x => x.MethodName == methodName);
        }

        public GeneratedMethod AddVoidMethod(string name, params Argument[] args)
        {
            var method = new GeneratedMethod(this, name, typeof(void), args);
            AddMethod(method);

            return method;
        }

        public GeneratedMethod AddMethodThatReturns<TReturn>(string name, params Argument[] args)
        {
            var method = new GeneratedMethod(this, name, typeof(TReturn), args);
            AddMethod(method);

            return method;
        }

        public void Write(ISourceWriter writer)
        {
            // We MUST generate the methods first, because during the writing of a method
            // it is possible it will find injected / static fields that are added to
            // this type that need to be written out below
            var methodWriter = new SourceWriter();

            foreach (var method in methods)
            {
                methodWriter.BlankLine();
                method.WriteMethod(methodWriter);
            }

            WriteDeclaration(writer);

            if (AllStaticFields.Any())
            {
                WriteFieldDeclarations(writer, AllStaticFields);
            }

            if (AllInjectedFields.Any())
            {
                WriteFieldDeclarations(writer, AllInjectedFields);
                WriteConstructorMethod(writer, AllInjectedFields);
            }

            writer.WriteLines(methodWriter.Code());

            writer.FinishBlock();
        }

        public IEnumerable<Assembly> AssemblyReferences()
        {
            if (BaseType != null)
            {
                yield return BaseType.Assembly;
            }

            foreach (var @interface in interfaces)
            {
                yield return @interface.Assembly;
            }
        }

        public T CreateInstance<T>(params object[] arguments)
        {
            if (CompiledType == null)
            {
                throw new InvalidOperationException("This generated assembly has not yet been successfully compiled");
            }

            return (T)Activator.CreateInstance(CompiledType, arguments);
        }

        public void ApplySetterValues(object builtObject)
        {
            if (builtObject.GetType() != CompiledType)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(builtObject),
                    "This can only be applied to objects of the generated type");
            }

            foreach (var setter in Setters)
            {
                setter.SetInitialValue(builtObject);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return GetDeclaration();
        }

        internal void FindType(Type[] generated)
        {
            CompiledType = generated.SingleOrDefault(x => x.Name == TypeName && x.Namespace == Namespace);

            if (CompiledType == null)
            {
                throw new InvalidOperationException($"Could not find compile typed {Namespace}.{TypeName} in {generated.Length} types");
            }
        }

        Variable IVariableSource.TryFindVariable(IMethodVariables variables, Type variableType)
        {
            foreach (var v in AllInjectedFields)
            {
                if (v.VariableType == variableType)
                {
                    return v;
                }
            }

            foreach (var v in BaseConstructorArguments)
            {
                if (v.VariableType == variableType)
                {
                    return v;
                }
            }

            return null;
        }

        private void WriteSetters(ISourceWriter writer)
        {
            foreach (var setter in Setters)
            {
                writer.BlankLine();
                setter.WriteDeclaration(writer);
            }

            writer.BlankLine();
        }

        private void WriteConstructorMethod(ISourceWriter writer, IList<InjectedField> args)
        {
            IEnumerable<string> tempQualifier = args.Select(x => x.CtorArgDeclaration);
            var ctorArgs = string.Join(", ", tempQualifier);
            var declaration = $"public {TypeName}({ctorArgs})";

            if (BaseConstructorArguments.Any())
            {
                IEnumerable<string> tempQualifier1 = BaseConstructorArguments.Select(x => x.ArgumentName);
                declaration = $"{declaration} : base({string.Join(", ", tempQualifier1)})";
            }

            writer.Block(declaration);

            foreach (var field in args)
            {
                field.WriteAssignment(writer);
            }

            writer.FinishBlock();
        }

        private void WriteFieldDeclarations(ISourceWriter writer, IList<StaticField> args)
        {
            foreach (var field in args)
            {
                field.WriteDeclaration(writer);
            }

            writer.BlankLine();
        }

        private void WriteFieldDeclarations(ISourceWriter writer, IList<InjectedField> args)
        {
            foreach (var field in args)
            {
                field.WriteDeclaration(writer);
            }

            writer.BlankLine();
        }

        private void WriteDeclaration(ISourceWriter writer)
        {
            writer.Block($"{GetDeclaration()}");
        }

        private string GetDeclaration()
        {
            var implemented = Implements().ToArray();

            if (implemented.Any())
            {
                IEnumerable<string> tempQualifier = implemented.Select(x => x.FullNameInCode());
                return $"public class {TypeName} : {string.Join(", ", tempQualifier)}";
            }

            return $":public class {TypeName}";
        }

        private IEnumerable<Type> Implements()
        {
            if (BaseType != null)
            {
                yield return BaseType;
            }

            foreach (var @interface in Interfaces)
            {
                yield return @interface;
            }
        }
    }
}
