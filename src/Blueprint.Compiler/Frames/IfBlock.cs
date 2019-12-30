using System;
using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    /// <summary>
    /// Represents an <code>if</code> statement, containing an expression (or <see cref="Variable"/>) that if evaluating to <code>true</code>
    /// at runtime runs the 'inner' frames of this <see cref="CompositeFrame" />.
    /// </summary>
    public class IfBlock : CompositeFrame
    {
        private readonly string condition;

        /// <summary>
        /// Initialises a new instance of an <see cref="IfBlock"/>, using the specified <see cref="Variable"/>.
        /// </summary>
        /// <param name="variable">A boolean variable.</param>
        /// <param name="inner">The (optional) set of frames to be the inner of this block. Note the preferred way of
        /// creating composite blocks is using inner list initialisation syntax.</param>
        public IfBlock(Variable variable, params Frame[] inner) : this(variable.Usage, inner)
        {
            if (variable.VariableType != typeof(bool))
            {
                throw new ArgumentException($"Variable must be of type boolean, but was {variable.VariableType}", nameof(variable));
            }
        }

        /// <summary>
        /// Initialises a new instance of an <see cref="IfBlock"/>, using the specified condition.
        /// </summary>
        /// <param name="condition">The code that will be placed in the if statement.</param>
        /// <param name="inner">The (optional) set of frames to be the inner of this block. Note the preferred way of
        /// creating composite blocks is using inner list initialisation syntax.</param>
        public IfBlock(string condition, params Frame[] inner) : base(inner)
        {
            this.condition = condition;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"if ({condition})";
        }

        /// <inheritdoc />
        protected override void GenerateCode(GeneratedMethod method, ISourceWriter writer, Frame inner)
        {
            writer.Write($"BLOCK:if ({condition})");
            inner.GenerateCode(method, writer);
            writer.FinishBlock();
        }
    }
}
