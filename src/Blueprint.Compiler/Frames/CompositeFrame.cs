using System;
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

        /// <summary>
        /// Gets a value indicating whether this <see cref="CompositeFrame"/> is async, determined by whether
        /// any of the children <see cref="Frame.IsAsync"/> properties are <c>true</c>.
        /// </summary>
        public override bool IsAsync => inner.Any(i => i.IsAsync);

        public List<Frame> Inner
        {
            get => inner;
        }

        public void Add(Frame innerFrame)
        {
            inner.Add(innerFrame);
        }

        public override bool CanReturnTask()
        {
            return inner.Last().CanReturnTask();
        }

        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            if (inner.Count > 1)
            {
                for (var i = 1; i < inner.Count; i++)
                {
                    inner[i - 1].NextFrame = inner[i];
                }
            }

            GenerateCode(variables, method, writer, inner[0]);

            next();
        }

        IEnumerator<Frame> IEnumerable<Frame>.GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        protected abstract void GenerateCode(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Frame inner);
    }
}
