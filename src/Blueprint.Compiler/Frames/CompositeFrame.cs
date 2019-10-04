using System.Collections.Generic;
using System.Linq;

using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    public abstract class CompositeFrame : Frame
    {
        private readonly Frame[] inner;

        protected CompositeFrame(params Frame[] inner) : base(inner.Any(x => x.IsAsync))
        {
            this.inner = inner;
        }

        public override IEnumerable<Variable> Creates => inner.SelectMany(x => x.Creates).ToArray();

        public sealed override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            if (inner.Length > 1)
            {
                for (var i = 1; i < inner.Length; i++)
                {
                    inner[i - 1].Next = inner[i];
                }
            }

            GenerateCode(method, writer, inner[0]);

            Next?.GenerateCode(method, writer);
        }

        public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            return inner.SelectMany(x => x.FindVariables(chain)).Distinct();
        }

        public override bool CanReturnTask()
        {
            return inner.Last().CanReturnTask();
        }

        protected abstract void GenerateCode(GeneratedMethod method, ISourceWriter writer, Frame inner);
    }
}
