using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler.Model
{
    internal class MethodFrameArranger : IMethodVariables
    {
        private readonly GeneratedMethod method;
        private readonly GeneratedType type;
        private readonly Dictionary<Type, Variable> variables = new Dictionary<Type, Variable>();

        public MethodFrameArranger(GeneratedMethod method, GeneratedType type)
        {
            this.method = method;
            this.type = type;
        }

        public void Arrange(out AsyncMode asyncMode, out Frame topFrame)
        {
            var compiled = CompileFrames(method.Frames);

            asyncMode = AsyncMode.AsyncTask;

            if (compiled.All(x => !x.IsAsync))
            {
                asyncMode = AsyncMode.None;
            }
            else if (compiled.Count(x => x.IsAsync) == 1 && compiled.Last().IsAsync && compiled.Last().CanReturnTask())
            {
                asyncMode = compiled.Any(x => x.Wraps) ? AsyncMode.AsyncTask : AsyncMode.ReturnFromLastNode;
            }

            topFrame = ChainFrames(compiled);
        }


        private Frame ChainFrames(Frame[] frames)
        {
            // Step 5, put into a chain.
            for (var i = 1; i < frames.Length; i++)
            {
                frames[i - 1].Next = frames[i];
            }

            return frames[0];
        }

        private Frame[] CompileFrames(IList<Frame> frames)
        {
            // Step 1, resolve all the necessary variables
            foreach (var frame in frames)
            {
                frame.ResolveVariables(this);
            }

            // Step 1, calculate dependencies
            var dependencies = new DependencyGatherer(this, frames);
            FindStaticFields(dependencies);
            FindInjectedFields(dependencies);
            FindSetters(dependencies);

            // Step 2, gather any missing frames and
            // add to the beginning of the list
            foreach (var x in dependencies.Dependencies.GetAll().SelectMany(x => x).Distinct()
                .Where(x => !frames.Contains(x)))
            {
                frames.Insert(0, x);
            }

            // Step 3, topological sort in dependency order
            return frames.TopologicalSort(x => dependencies.Dependencies[x], true).ToArray();
        }

        private void FindInjectedFields(DependencyGatherer dependencies)
        {
            dependencies.Variables.Each((key, _) =>
            {
                if (key is InjectedField field)
                {
                    type.AllInjectedFields.Fill(field);
                }
            });
        }

        private void FindStaticFields(DependencyGatherer dependencies)
        {
            dependencies.Variables.Each((key, _) =>
            {
                if (key is StaticField field)
                {
                    type.AllStaticFields.Fill(field);
                }
            });
        }

        private void FindSetters(DependencyGatherer dependencies)
        {
            dependencies.Variables.Each((key, _) =>
            {
                if (key is Setter setter)
                {
                    type.Setters.Fill(setter);
                }
            });
        }

        private IEnumerable<IVariableSource> AllVariableSources()
        {
            foreach (var source in method.Sources)
            {
                yield return source;
            }

            // To get injected fields
            yield return type;
        }

        private Variable FindVariable(Type type, VariableSource variableSource)
        {
            var argument = method.Arguments.Concat(method.DerivedVariables).Concat(this.type.AllStaticFields).FirstOrDefault(x => x.VariableType == type);
            if (argument != null)
            {
                return argument;
            }

            var created = method.Frames.SelectMany(x => x.Creates).FirstOrDefault(x => x.VariableType == type);
            if (created != null)
            {
                return created;
            }

            var source = AllVariableSources().FirstOrDefault(x => x.Matches(type));
            return source?.Create(type);
        }

        public Variable FindVariableByName(Type dependency, string name)
        {
            if (TryFindVariableByName(dependency, name, out var variable)) return variable;

            throw new ArgumentOutOfRangeException(nameof(dependency), $"Cannot find a matching variable {dependency.FullName} {name}");
        }

        public Variable FindVariable(Type type)
        {
            if (variables.ContainsKey(type))
            {
                return variables[type];
            }

            var variable = FindVariable(type, VariableSource.All);
            if (variable == null)
            {
                throw new ArgumentOutOfRangeException(nameof(type),
                    $"Do not know how to build a variable of type '{type.FullName}'");
            }

            variables.Add(type, variable);

            return variable;
        }

        public bool TryFindVariableByName(Type dependency, string name, out Variable variable)
        {
            variable = null;

            // It's fine here for now that we aren't looking through the services for
            // variables that could potentially be built by the IoC container
            var sourced = method.Sources.Where(x => x.Matches(dependency)).Select(x => x.Create(dependency));
            var created = method.Frames.SelectMany(x => x.Creates);

            var candidate = variables.Values
                .Concat(method.Arguments)
                .Concat(method.DerivedVariables)
                .Concat(created)
                .Concat(sourced)
                .Where(x => x != null)
                .FirstOrDefault(x => x.VariableType == dependency && x.Usage == name);

            if (candidate != null)
            {
                variable = candidate;
                return true;
            }

            return false;
        }

        public Variable TryFindVariable(Type type, VariableSource source)
        {
            if (variables.ContainsKey(type))
            {
                return variables[type];
            }

            var variable = FindVariable(type, source);
            if (variable != null)
            {
                variables.Add(type, variable);
            }

            return variable;
        }
    }
}
