using System;
using System.Linq;
using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    /// <summary>
    /// A <see cref="Frame" /> that can be added but will conditionally add a number of child frames based on a predicate
    /// that can make a decision given the <see cref="IMethodVariables" /> and <see cref="GeneratedMethod" />.
    /// </summary>
    /// <remarks>
    /// This is useful when, for example, a section of code should be added only based on the configured variables of
    /// other areas of a <see cref="GeneratedMethod" /> but is not known at the time of adding frames.
    /// </remarks>
    public class ConditionalFrame : Frame
    {
        private readonly Func<IMethodVariables, GeneratedMethod, bool> _predicate;
        private readonly Frame[] _childFrames;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalFrame"/> class.
        /// </summary>
        /// <param name="predicate">The predicate that, if returning <c>true</c>, means the given frames should be rendered.</param>
        /// <param name="childFrames">The child frames to append.</param>
        public ConditionalFrame(
            Func<IMethodVariables, GeneratedMethod, bool> predicate,
            params Frame[] childFrames)
            : base(false)
        {
            this._predicate = predicate;
            this._childFrames = childFrames;
        }

        /// <summary>
        /// Returns <c>true</c> if any of the child frames are async.
        /// </summary>
        public override bool IsAsync => this._childFrames.Any(f => f.IsAsync);

        /// <inheritdoc/>
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            if (this._predicate(variables, method))
            {
                foreach (var f in this._childFrames)
                {
                    writer.Write(f);
                }
            }

            next();
        }
    }
}
