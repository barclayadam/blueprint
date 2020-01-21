using System.Collections.Generic;
using System.Linq;

using Blueprint.Compiler.Frames;
using Blueprint.Compiler.Model;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler
{
    internal class DependencyGatherer
    {
        private readonly LightweightCache<Frame, List<Frame>> dependencies = new LightweightCache<Frame, List<Frame>>();
        private readonly LightweightCache<Variable, List<Frame>> variables = new LightweightCache<Variable, List<Frame>>();

        private readonly IMethodVariables methodVariables;

        public DependencyGatherer(IMethodVariables methodVariables, IList<Frame> frames)
        {
            this.methodVariables = methodVariables;
            dependencies.OnMissing = frame => new List<Frame>(FindDependencies(frame).Distinct());
            variables.OnMissing = v => new List<Frame>(FindDependencies(v).Distinct());

            foreach (var frame in frames)
            {
                dependencies.FillDefault(frame);
            }
        }

        public LightweightCache<Frame, List<Frame>> Dependencies
        {
            get => dependencies;
        }

        public LightweightCache<Variable, List<Frame>> Variables
        {
            get => variables;
        }

        private IEnumerable<Frame> FindDependencies(Frame frame)
        {
            // frame.ResolveVariables(methodVariables);

            foreach (var variable in frame.Uses)
            {
                foreach (var dependency in variables[variable])
                {
                    yield return dependency;
                }
            }
        }

        private IEnumerable<Frame> FindDependencies(Variable variable)
        {
            if (variable.Creator != null)
            {
                yield return variable.Creator;
                foreach (var frame in dependencies[variable.Creator])
                {
                    yield return frame;
                }
            }

            foreach (var dependency in variable.Dependencies)
            {
                foreach (var frame in variables[dependency])
                {
                    yield return frame;
                }
            }
        }
    }
}
