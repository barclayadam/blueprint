using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler.Frames
{
    /// <summary>
    /// A frame that is used to call a constructor to create a new instance
    /// of a class.
    /// </summary>
    public class ConstructorFrame : SyncFrame
    {
        internal ConstructorFrame(Type builtType, ConstructorInfo ctor)
        {
            this.Ctor = ctor ?? throw new ArgumentNullException(nameof(ctor));
            this.Parameters = new Variable[ctor.GetParameters().Length];

            this.BuiltType = builtType;
            this.Variable = new Variable(this.BuiltType, this);
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ConstructorFrame" />
        /// using the given <see cref="ConstructorInfo" />
        /// </summary>
        /// <param name="ctor">The constructor to be called.</param>
        public ConstructorFrame(ConstructorInfo ctor)
            : this(ctor.DeclaringType, ctor)
        {
        }

        /// <summary>
        /// The type being built.
        /// </summary>
        public Type BuiltType { get;  }

        /// <summary>
        /// An optional type that can be used to specify the type of the
        /// variable created, instead of using <c>var</c> (i.e. if this is set
        /// the output will be <c>[DeclaringType] [VariableName] = new [CtorCall]</c>).
        /// </summary>
        public Type? DeclaredType { get; set; }

        /// <summary>
        /// The acual constructor to be used.
        /// </summary>
        internal ConstructorInfo Ctor { get; }

        /// <summary>
        /// The parameters to the constructor, matching the positional
        /// arguments.
        /// </summary>
        public Variable[] Parameters { get; }

        public FramesCollection ActivatorFrames { get; } = new FramesCollection();

        /// <summary>
        /// Determines the way the constructor call will be output.
        /// </summary>
        public ConstructorCallMode Mode { get; set; } = ConstructorCallMode.Variable;

        /// <summary>
        /// A list of setters that can be used to set properties of the
        /// created instance.
        /// </summary>
        public IList<SetterArg> Setters { get; } = new List<SetterArg>();

        /// <summary>
        /// Gets or sets the variable set by invoking this frame.
        /// </summary>
        public Variable Variable { get; protected set; }

        /// <inheritdoc />
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            var parameters = this.Ctor.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                if (this.Parameters[i] == null)
                {
                    var parameter = parameters[i];
                    this.Parameters[i] = variables.FindVariable(parameter.ParameterType);
                }
            }

            foreach (var setter in this.Setters)
            {
                setter.FindVariable(variables);
            }

            switch (this.Mode)
            {
                case ConstructorCallMode.Variable:
                    writer.WriteLine(this.Declaration() + ";");
                    this.ActivatorFrames.Write(variables, method, writer);

                    next();
                    break;

                case ConstructorCallMode.ReturnValue:
                    if (this.ActivatorFrames.Any())
                    {
                        writer.WriteLine(this.Declaration() + ";");
                        this.ActivatorFrames.Write(variables, method, writer);

                        writer.WriteLine($"return {this.Variable};");
                        next();
                    }
                    else
                    {
                        writer.WriteLine($"return {this.Invocation()};");
                        next();
                    }

                    break;

                case ConstructorCallMode.UsingNestedVariable:
                    writer.Using(this.Declaration(), w =>
                    {
                        this.ActivatorFrames.Write(variables, method, writer);

                        next();
                    });
                    break;
            }
        }

        /// <summary>
        /// The declaration code, which includes the setting of the <see cref="Variable" />
        /// (using <c>var</c> or the <see cref="DeclaredType" />) plus the actual
        /// invocation of the constructor.
        /// </summary>
        /// <returns>The code used to set a variable with result of this constructor call.</returns>
        public string Declaration()
        {
            return this.DeclaredType == null
                ? $"var {this.Variable} = {this.Invocation()}"
                : $"{this.DeclaredType.FullNameInCode()} {this.Variable} = {this.Invocation()}";
        }

        /// <summary>
        /// The invocation of this constructor (i.e. the <c>new</c> construct), plus
        /// any setters.
        /// </summary>
        /// <returns>The constructor invocation.</returns>
        public string Invocation()
        {
            var arguments = this.Parameters.Select(x => x.Usage);
            var invocation = $"new {this.BuiltType.FullNameInCode()}({string.Join(", ", arguments)})";

            if (this.Setters.Any())
            {
                var setterInvocations = this.Setters.Select(x => x.Assignment());
                invocation += $"{{{string.Join(", ", setterInvocations)}}}";
            }

            return invocation;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.DeclaredType == null
                ? $"var {this.Variable} = new {this.BuiltType.Name}(...);"
                : $"{this.DeclaredType.FullNameInCode()} {this.Variable} = new {this.BuiltType.Name}(...);";
        }
    }

    /// <summary>
    /// A typed <see cref="ConstructorFrame" />.
    /// </summary>
    /// <typeparam name="T">The built type.</typeparam>
    public class ConstructorFrame<T> : ConstructorFrame
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="ConstructorFrame{T}" />
        /// class from the given constructor info.
        /// </summary>
        /// <param name="ctor">The constructor info.</param>
        public ConstructorFrame(ConstructorInfo ctor)
            : base(typeof(T), ctor)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="ConstructorFrame{T}" />
        /// class by an Expression that represents a constructor call.
        /// </summary>
        /// <param name="expression">An expression that represents a constructor call.</param>
        public ConstructorFrame(Expression<Func<T>> expression)
            : base(typeof(T), ConstructorFinderVisitor<T>.Find(expression))
        {
        }

        /// <summary>
        /// Sets a property, at construction time, to the result of the given variable.
        /// </summary>
        /// <param name="expression">An expression that is a property reference (i.e. t => t.[PropertyName]).</param>
        /// <param name="variable">The variable to set the property tp.</param>
        public void Set(Expression<Func<T, object>> expression, Variable variable = null)
        {
            var property = ReflectionHelper.GetProperty(expression);
            var setter = new SetterArg(property, variable);

            this.Setters.Add(setter);
        }
    }
}
