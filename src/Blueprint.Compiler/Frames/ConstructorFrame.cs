using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler.Frames
{
    public class ConstructorFrame : SyncFrame
    {
        public ConstructorFrame(ConstructorInfo ctor)
            : this(ctor.DeclaringType, ctor)
        {
        }

        public ConstructorFrame(Type builtType, ConstructorInfo ctor)
        {
            this.Ctor = ctor ?? throw new ArgumentNullException(nameof(ctor));
            this.Parameters = new Variable[ctor.GetParameters().Length];

            this.BuiltType = builtType;
            this.Variable = new Variable(this.BuiltType, this);
        }

        public Type BuiltType { get;  }

        public Type DeclaredType { get; set; }

        public ConstructorInfo Ctor { get; }

        public Variable[] Parameters { get; }

        public FramesCollection ActivatorFrames { get; } = new FramesCollection();

        public ConstructorCallMode Mode { get; set; } = ConstructorCallMode.Variable;

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

        public string Declaration()
        {
            return this.DeclaredType == null
                ? $"var {this.Variable} = {this.Invocation()}"
                : $"{this.DeclaredType.FullNameInCode()} {this.Variable} = {this.Invocation()}";
        }

        public string Invocation()
        {
            var tempQualifier = this.Parameters.Select(x => x.Usage);
            var invocation = $"new {this.BuiltType.FullNameInCode()}({string.Join(", ", tempQualifier)})";
            if (this.Setters.Any())
            {
                var tempQualifier1 = this.Setters.Select(x => x.Assignment());
                invocation += $"{{{string.Join(", ", tempQualifier1)}}}";
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

    public class ConstructorFrame<T> : ConstructorFrame
    {
        public ConstructorFrame(ConstructorInfo ctor)
            : base(typeof(T), ctor)
        {
        }

        public ConstructorFrame(Expression<Func<T>> expression)
            : base(typeof(T), ConstructorFinderVisitor<T>.Find(expression))
        {
        }

        public void Set(Expression<Func<T, object>> expression, Variable variable = null)
        {
            var property = ReflectionHelper.GetProperty(expression);
            var setter = new SetterArg(property, variable);

            this.Setters.Add(setter);
        }
    }
}
