using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler;

/// <summary>
/// Represents a type / class that is being constructed.
/// </summary>
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

    /// <summary>
    /// A list of properties of this type.
    /// </summary>
    public IList<Property> Properties { get; } = new List<Property>();

    /// <summary>
    /// The namespace this type belongs to.
    /// </summary>
    public string Namespace { get; set; }

    /// <summary>
    /// The name of this generated name.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// The base type of this, if any.
    /// </summary>
    public Type? BaseType { get; private set; }

    /// <summary>
    /// A set of <see cref="InjectedField" />s that represent the constructor arguments of the
    /// base class of this type, if any.
    /// </summary>
    public InjectedField[]? BaseConstructorArguments { get; private set; } = Array.Empty<InjectedField>();

    /// <summary>
    /// A set of namespaces that this type uses and that will be output as a set of <c>using</c> statements.
    /// </summary>
    public HashSet<string> UsingNamespaces { get; } = new ();

    /// <summary>
    /// All fields that are injected in to this type through one of it's constructors.
    /// </summary>
    public HashSet<InjectedField> AllInjectedFields { get; } = new ();

    /// <summary>
    /// All static fields of this type.
    /// </summary>
    public HashSet<StaticField> AllStaticFields { get; } = new ();

    /// <summary>
    /// The interfaces this type implements.
    /// </summary>
    public IEnumerable<Type> Interfaces => this._interfaces;

    /// <summary>
    /// The set of methods added to this type.
    /// </summary>
    public IEnumerable<GeneratedMethod> Methods => this._methods;

    /// <summary>
    /// The generated source code of this type (set when the type has been "compiled").
    /// </summary>
    /// <seealso cref="Blueprint.Compiler.GeneratedAssembly.CompileAll" />
    public string? GeneratedSourceCode { get; set; }

    /// <summary>
    /// The runtime type, set once the type has been "compiled" and the generated assembly
    /// loaded in to the current app domain.
    /// </summary>
    /// <seealso cref="Blueprint.Compiler.GeneratedAssembly.CompileAll" />
    public Type? CompiledType { get; private set; }

    /// <summary>
    /// Marks this type as inheriting from the specified type parameter.
    /// </summary>
    /// <typeparam name="T">The type to inherit from.</typeparam>
    /// <returns>This <see cref="GeneratedType" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the base type has more than one public constructor.</exception>
    public GeneratedType InheritsFrom<T>()
    {
        return this.InheritsFrom(typeof(T));
    }

    /// <summary>
    /// Marks this type as inheriting from the specified type parameter.
    /// </summary>
    /// <param name="baseType">The type to inherit from.</param>
    /// <returns>This <see cref="GeneratedType" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the base type has more than one public constructor.</exception>
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

    /// <summary>
    /// Marks this type as implementing the given interface, adding a <see cref="GeneratedMethod" /> for each method
    /// declared in the interface (without a body).
    /// </summary>
    /// <param name="type">The interface to implement.</param>
    /// <returns>This <see cref="GeneratedType" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the given type is not an interface.</exception>
    public GeneratedType Implements(Type type)
    {
        if (!type.IsInterface)
        {
            throw new ArgumentOutOfRangeException(nameof(type), $"The type {type} must be an interface");
        }

        this._interfaces.Add(type);

        foreach (var methodInfo in type.GetMethods().Where(x => x.DeclaringType != typeof(object)))
        {
            this._methods.Add(new GeneratedMethod(this, methodInfo));
        }

        return this;
    }

    /// <summary>
    /// Marks this type as implementing the given interface, adding a <see cref="GeneratedMethod" /> for each method
    /// declared in the interface (without a body).
    /// </summary>
    /// <typeparam name="T">The interface to implement.</typeparam>
    /// <returns>This <see cref="GeneratedType" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the given type is not an interface.</exception>
    public GeneratedType Implements<T>()
    {
        return this.Implements(typeof(T));
    }

    /// <summary>
    /// Adds a new generated method to this type.
    /// </summary>
    /// <param name="method">The method to add.</param>
    public void AddMethod(GeneratedMethod method)
    {
        this._methods.Add(method);
    }

    /// <summary>
    /// Finds an existing <see cref="GeneratedMethod" /> with the given name.
    /// </summary>
    /// <param name="methodName">The method's name.</param>
    /// <returns>A <see cref="GeneratedMethod" /> that already exists.</returns>
    public GeneratedMethod? MethodFor(string methodName)
    {
        var ofGivenName = this._methods.Where(x => x.MethodName == methodName).ToArray();

        if (!ofGivenName.Any())
        {
            throw new ArgumentException($"No method with the name {methodName} exists on the generated type {this.Namespace}.{this.TypeName}.");
        }

        if (ofGivenName.Length > 1)
        {
            throw new ArgumentException($"More than one method with the name {methodName} exists on the generated type {this.Namespace}.{this.TypeName}.");
        }

        return ofGivenName.Single();
    }

    /// <summary>
    /// Adds a new <see cref="GeneratedMethod" /> of the given name that returns <c>void</c> and has the given
    /// list of <see cref="Argument" />s.
    /// </summary>
    /// <param name="name">The name of the method.</param>
    /// <param name="args">The arguments of the added method.</param>
    /// <returns>The newly added <see cref="GeneratedMethod" />.</returns>
    public GeneratedMethod AddVoidMethod(string name, params Argument[] args)
    {
        var method = new GeneratedMethod(this, name, typeof(void), args);
        this.AddMethod(method);

        return method;
    }

    /// <summary>
    /// Adds a new <see cref="GeneratedMethod" /> of the given name that returns <typeparamref name="TReturn" /> and has the given
    /// list of <see cref="Argument" />s.
    /// </summary>
    /// <typeparam name="TReturn">The type returned from the added method.</typeparam>
    /// <param name="name">The name of the method.</param>
    /// <param name="args">The arguments of the added method.</param>
    /// <returns>The newly added <see cref="GeneratedMethod" />.</returns>
    public GeneratedMethod AddMethodThatReturns<TReturn>(string name, params Argument[] args)
    {
        var method = new GeneratedMethod(this, name, typeof(TReturn), args);
        this.AddMethod(method);

        return method;
    }

    /// <summary>
    /// Writes the C# of this <see cref="GeneratedType" /> to the given <see cref="ISourceWriter" />.
    /// </summary>
    /// <param name="writer">The writer to write this type to.</param>
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
            WriteFieldDeclarations(writer, this.AllStaticFields);
        }

        if (this.AllInjectedFields.Any())
        {
            WriteFieldDeclarations(writer, this.AllInjectedFields);
            this.WriteConstructorMethod(writer, this.AllInjectedFields);
        }

        writer.WriteLines(methodWriter.Code());

        writer.FinishBlock();
    }

    /// <summary>
    /// Gets the assemblies that this type references, which is the base type's assembly plus all
    /// implemented interface assemblies.
    /// </summary>
    /// <returns>A set of assemblies that <strong>must</strong> be referenced by the resulting assembly.</returns>
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

    /// <summary>
    /// Once a type has been part of a compilation will create an instance, casting it to the
    /// specified type.
    /// </summary>
    /// <param name="arguments">Any constructor arguments.</param>
    /// <typeparam name="T">The type to cast the resulting instance to.</typeparam>
    /// <returns>A new instance of the type generated from this <see cref="GeneratedType" />.</returns>
    /// <exception cref="InvalidOperationException">If this type has yet to be compiled.</exception>
    public T CreateInstance<T>(params object[] arguments)
    {
        if (this.CompiledType == null)
        {
            throw new InvalidOperationException("This generated assembly has not yet been successfully compiled");
        }

        return (T)Activator.CreateInstance(this.CompiledType, arguments);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return this.GetDeclaration();
    }

    internal void FindType(Type[] generated)
    {
        this.CompiledType = generated.FirstOrDefault(x => x.Name == this.TypeName && x.Namespace == this.Namespace);

        if (this.CompiledType == null)
        {
            throw new InvalidOperationException($"Could not find compile typed {this.Namespace}.{this.TypeName} in {generated.Length} types");
        }
    }

    /// <inheritdoc/>
    Variable IVariableSource.TryFindVariable(IMethodVariables variables, Type variableType)
    {
        foreach (var v in this.AllInjectedFields)
        {
            if (v.VariableType == variableType)
            {
                return v;
            }
        }

        if (this.BaseConstructorArguments != null)
        {
            foreach (var v in this.BaseConstructorArguments)
            {
                if (v.VariableType == variableType)
                {
                    return v;
                }
            }
        }

        return null;
    }

    private static void WriteFieldDeclarations(ISourceWriter writer, HashSet<StaticField> args)
    {
        foreach (var field in args)
        {
            field.WriteDeclaration(writer);
        }

        writer.BlankLine();
    }

    private static void WriteFieldDeclarations(ISourceWriter writer, HashSet<InjectedField> args)
    {
        foreach (var field in args)
        {
            field.WriteDeclaration(writer);
        }

        writer.BlankLine();
    }

    private void WriteSetters(ISourceWriter writer)
    {
        foreach (var setter in this.Properties)
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

        if (this.BaseConstructorArguments?.Any() == true)
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