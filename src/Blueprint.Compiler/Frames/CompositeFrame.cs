using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    public abstract class CompositeFrame : Frame, IEnumerable<Frame>
    {
        private readonly List<Frame> _inner;

        protected CompositeFrame(params Frame[] inner) : base(inner.Any(x => x.Is))
        {
            this._inner = inner.ToList();
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="CompositeFrame"/> is async, determined by whether
        /// any of the children <see cref="Frame.Is"/> properties are <c>true</c>.
        /// </summary>
        public override bool Is => this._inner.Any(i => i.Is);

        public List<Frame> Inner => this._inner;

        public void Add(Frame innerFrame)
        {
            this._inner.Add(innerFrame);
        }

        public override bool CanReturnTask()
        {
            return this._inner.Last().CanReturnTask();
        }

        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            if (this._inner.Count > 1)
            {
                for (var i = 1; i < this._inner.Count; i++)
                {
                    this._inner[i - 1].NextFrame = this._inner[i];
                }
            }

            this.GenerateCode(variables, method, writer, this._inner[0]);

            next();
        }

        IEnumerator<Frame> IEnumerable<Frame>.GetEnumerator()
        {
            return this._inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._inner.GetEnumerator();
        }

        protected abstract void GenerateCode(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Frame inner);
    }
}
