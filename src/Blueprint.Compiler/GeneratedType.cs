using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Blueprint.Compiler.Model;

namespace Blueprint.Compiler
{
    public class GeneratedType : IVariableSource
    {
        private readonly IList<Type> _interfaces = new List<Type>();
        private readonly IList<GeneratedMethod> _methods = new List<GeneratedMethod>();

        internal GeneratedType(GeneratedAssembly generatedAssembly, string typeName, string @namespace)
        {
            this.GeneratedAssembly = generatedAssembly;

            this.Namespace = @namespace;
            this.TypeName = typeName;
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

        public HashSet<InjectedField> AllInjectedFields { get; } = new HashSet<InjectedField>();

        public HashSet<StaticField> AllStaticFields { get; } = new HashSet<StaticField>();

        public IEnumerable<Type> Interfaces => this._interfaces;

        public IEnumerable<GeneratedMethod> Methods => this._methods;

        public string SourceCode { get; set; }

        public Type CompiledType { get; private set; }

        public GeneratedType InheritsFrom<T>()
        {
            return this.InheritsFrom(typeof(T));
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
                this.BaseConstructorArguments = ctors.Single().GetParameters()
                    .Select(x => new InjectedField(x.ParameterType, x.Name)).ToArray();

                foreach (var a in this.BaseConstructorArguments)
                {
                    this.AllInjectedFields.Add(a);
                }
            }

            this.BaseType = baseType;

            foreach (var methodInfo in baseType.GetMethods().Where(x => x.DeclaringType != typeof(object)).Where(x => x.CanBeOverridden()))
            {
                this._methods.Add(new GeneratedMethod(this, methodInfo)
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

            this._interfaces.Add(type);

            foreach (var methodInfo in type.GetMethods().Where(x => x.DeclaringType != typeof(object)))
            {
                this._methods.Add(new GeneratedMethod(this, methodInfo));
            }

            return this;
        }

        public GeneratedType Implements<T>()
        {
            return this.Implements(typeof(T));
        }

        public void AddMethod(GeneratedMethod method)
        {
            this._methods.Add(method);
        }

        public GeneratedMethod MethodFor(string methodName)
        {
            return this._methods.FirstOrDefault(x => x.MethodName == methodName);
        }

        public GeneratedMethod AddVoidMethod(string name, params Argument[] args)
        {
            var method = new GeneratedMethod(this, name, typeof(void), args);
            this.AddMethod(method);

            return method;
        }

        public GeneratedMethod AddMethodThatReturns<TReturn>(string name, params Argument[] args)
        {
            var method = new GeneratedMethod(this, name, typeof(TReturn), args);
            this.AddMethod(method);

            return method;
        }

        public void Write(ISourceWriter writer)
        {
            // We MUST generate the methods first, because during the writing of a method
            // it is possible it will find injected / static fields that are added to
            // this type that need to be written out below
            var methodWriter = new SourceWriter();

            foreach (var method in this._methods)
            {
                methodWriter.BlankLine();
                method.WriteMethod(methodWriter);
            }

            this.WriteDeclaration(writer);

            if (this.AllStaticFields.Any())
            {
                this.WriteFieldDeclarations(writer, this.AllStaticFields);
            }

            if (this.AllInjectedFields.Any())
            {
                this.WriteFieldDeclarations(writer, this.AllInjectedFields);
                this.WriteConstructorMethod(writer, this.AllInjectedFields);
            }

            writer.WriteLines(methodWriter.Code());

            writer.FinishBlock();
        }

        public IEnumerable<Assembly> AssemblyReferences()
        {
            if (this.BaseType != null)
            {
                yield return this.BaseType.Assembly;
            }

            foreach (var @interface in this._interfaces)
            {
                yield return @interface.Assembly;
            }
        }

        public T CreateInstance<T>(params object[] arguments)
        {
            if (this.CompiledType == null)
            {
                throw new InvalidOperationException("This generated assembly has not yet been successfully compiled");
            }

            return (T)Activator.CreateInstance(this.CompiledType, arguments);
        }

        public void ApplySetterValues(object builtObject)
        {
            if (builtObject.GetType() != this.CompiledType)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(builtObject),
                    "This can only be applied to objects of the generated type");
            }

            foreach (var setter in this.Setters)
            {
                setter.SetInitialValue(builtObject);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.GetDeclaration();
        }

        internal void FindType(Type[] generated)
        {
            this.CompiledType = generated.SingleOrDefault(x => x.Name == this.TypeName && x.Namespace == this.Namespace);

            if (this.CompiledType == null)
            {
                throw new InvalidOperationException($"Could not find compile typed {this.Namespace}.{this.TypeName} in {generated.Length} types");
            }
        }

        Variable IVariableSource.TryFindVariable(IMethodVariables variables, Type variableType)
        {
            foreach (var v in this.AllInjectedFields)
            {
                if (v.VariableType == variableType)
                {
                    return v;
                }
            }

            foreach (var v in this.BaseConstructorArguments)
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
            foreach (var setter in this.Setters)
            {
                writer.BlankLine();
                setter.WriteDeclaration(writer);
            }

            writer.BlankLine();
        }

        private void WriteConstructorMethod(ISourceWriter writer, HashSet<InjectedField> args)
        {
            var tempQualifier = args.Select(x => x.CtorArgDeclaration);
            var ctorArgs = string.Join(", ", tempQualifier);
            var declaration = $"public {this.TypeName}({ctorArgs})";

            if (this.BaseConstructorArguments.Any())
            {
                var tempQualifier1 = this.BaseConstructorArguments.Select(x => x.ArgumentName);
                declaration = $"{declaration} : base({string.Join(", ", tempQualifier1)})";
            }

            writer.Block(declaration);

            foreach (var field in args)
            {
                field.WriteAssignment(writer);
            }

            writer.FinishBlock();
        }

        private void WriteFieldDeclarations(ISourceWriter writer, HashSet<StaticField> args)
        {
            foreach (var field in args)
            {
                field.WriteDeclaration(writer);
            }

            writer.BlankLine();
        }

        private void WriteFieldDeclarations(ISourceWriter writer, HashSet<InjectedField> args)
        {
            foreach (var field in args)
            {
                field.WriteDeclaration(writer);
            }

            writer.BlankLine();
        }

        private void WriteDeclaration(ISourceWriter writer)
        {
            writer.Block($"{this.GetDeclaration()}");
        }

        private string GetDeclaration()
        {
            var implemented = this.Implements().ToArray();

            if (implemented.Any())
            {
                var tempQualifier = implemented.Select(x => x.FullNameInCode());
                return $"public class {this.TypeName} : {string.Join(", ", tempQualifier)}";
            }

            return $":public class {this.TypeName}";
        }

        private IEnumerable<Type> Implements()
        {
            if (this.BaseType != null)
            {
                yield return this.BaseType;
            }

            foreach (var @interface in this.Interfaces)
            {
                yield return @interface;
            }
        }
    }
}
