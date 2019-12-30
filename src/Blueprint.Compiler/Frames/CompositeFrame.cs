using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    public abstract class CompositeFrame : Frame, IEnumerable<Frame>
    {
        private readonly List<Frame> inner;

        protected CompositeFrame(params Frame[] inner) : base(inner.Any(x => x.IsAsync))
        {
            this.inner = inner.ToList();
        }

        public override IEnumerable<Variable> Creates => inner.SelectMany(x => x.Creates).ToArray();

        public void Add(Frame innerFrame)
        {
            inner.Add(innerFrame);
        }

        public sealed override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            if (inner.Count > 1)
            {
                for (var i = 1; i < inner.Count; i++)
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

        IEnumerator<Frame> IEnumerable<Frame>.GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        protected abstract void GenerateCode(GeneratedMethod method, ISourceWriter writer, Frame inner);
    }
}
