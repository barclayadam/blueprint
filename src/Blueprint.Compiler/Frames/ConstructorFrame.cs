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
        public ConstructorFrame(ConstructorInfo ctor) : this(ctor.DeclaringType, ctor)
        {
        }

        public ConstructorFrame(Type builtType, ConstructorInfo ctor)
        {
            Ctor = ctor ?? throw new ArgumentNullException(nameof(ctor));
            Parameters = new Variable[ctor.GetParameters().Length];

            BuiltType = builtType;
            Variable = new Variable(BuiltType, this);
        }

        public ConstructorFrame(Type builtType, ConstructorInfo ctor, Func<ConstructorFrame, Variable> variableSource)
        {
            Ctor = ctor ?? throw new ArgumentNullException(nameof(ctor));
            Parameters = new Variable[ctor.GetParameters().Length];

            BuiltType = builtType;
            Variable = variableSource(this);
        }

        public Type BuiltType { get;  }

        public Type DeclaredType { get; set; }

        public ConstructorInfo Ctor { get; }

        public Variable[] Parameters { get; set; }

        public FramesCollection ActivatorFrames { get; } = new FramesCollection();

        public ConstructorCallMode Mode { get; set; } = ConstructorCallMode.Variable;

        public IList<SetterArg> Setters { get; } = new List<SetterArg>();

        /// <summary>
        /// Gets or sets the variable set by invoking this frame.
        /// </summary>
        public Variable Variable { get; protected set; }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            switch (Mode)
            {
                case ConstructorCallMode.Variable:
                    writer.Write(Declaration() + ";");
                    ActivatorFrames.Write(method, writer);

                    Next?.GenerateCode(method, writer);
                    break;

                case ConstructorCallMode.ReturnValue:
                    if (ActivatorFrames.Any())
                    {
                        writer.Write(Declaration() + ";");
                        ActivatorFrames.Write(method, writer);

                        writer.Write($"return {Variable};");
                        Next?.GenerateCode(method, writer);
                    }
                    else
                    {
                        writer.Write($"return {Invocation()};");
                        Next?.GenerateCode(method, writer);
                    }

                    break;

                case ConstructorCallMode.UsingNestedVariable:
                    writer.UsingBlock(Declaration(), w =>
                    {
                        ActivatorFrames.Write(method, writer);
                        Next?.GenerateCode(method, w);
                    });
                    break;
            }
        }

        public string Declaration()
        {
            return DeclaredType == null
                ? $"var {Variable} = {Invocation()}"
                : $"{DeclaredType.FullNameInCode()} {Variable} = {Invocation()}";
        }

        public string Invocation()
        {
            var invocation = $"new {BuiltType.FullNameInCode()}({Parameters.Select(x => x.Usage).Join(", ")})";
            if (Setters.Any())
            {
                invocation += $"{{{Setters.Select(x => x.Assignment()).Join(", ")}}}";
            }

            return invocation;
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            var parameters = Ctor.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                if (Parameters[i] == null)
                {
                    var parameter = parameters[i];
                    Parameters[i] = chain.FindVariable(parameter.ParameterType);
                }
            }

            foreach (var parameter in Parameters)
            {
                yield return parameter;
            }

            foreach (var setter in Setters)
            {
                setter.FindVariable(chain);
            }

            foreach (var setter in Setters)
            {
                yield return setter.Variable;
            }

            if (ActivatorFrames.Any())
            {
                var standin = new StandinMethodVariables(Variable, chain);

                foreach (var frame in ActivatorFrames)
                {
                    foreach (var variable in frame.FindVariables(standin))
                    {
                        yield return variable;
                    }
                }
            }
        }

        public class StandinMethodVariables : IMethodVariables
        {
            private readonly Variable current;
            private readonly IMethodVariables inner;

            public StandinMethodVariables(Variable current, IMethodVariables inner)
            {
                this.current = current;
                this.inner = inner;
            }

            public Variable FindVariable(Type type)
            {
                return type == current.VariableType ? current : inner.FindVariable(type);
            }

            public Variable FindVariableByName(Type dependency, string name)
            {
                return inner.FindVariableByName(dependency, name);
            }

            public bool TryFindVariableByName(Type dependency, string name, out Variable variable)
            {
                return inner.TryFindVariableByName(dependency, name, out variable);
            }

            public Variable TryFindVariable(Type type)
            {
                return inner.TryFindVariable(type);
            }
        }
    }

    public class ConstructorFrame<T> : ConstructorFrame
    {
        public ConstructorFrame(ConstructorInfo ctor) : base(typeof(T), ctor)
        {
        }

        public ConstructorFrame(Expression<Func<T>> expression) : base(typeof(T), ConstructorFinderVisitor<T>.Find(expression))
        {
        }

        public void Set(Expression<Func<T, object>> expression, Variable variable = null)
        {
            var property = ReflectionHelper.GetProperty(expression);
            var setter = new SetterArg(property, variable);

            Setters.Add(setter);
        }
    }
}
