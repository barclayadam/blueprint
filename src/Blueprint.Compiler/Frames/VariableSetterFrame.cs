using System;
using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    /// <summary>
    /// A <see cref="SyncFrame" /> that will output code to set a variable to the value of another.
    /// </summary>
    public class VariableSetterFrame : SyncFrame
    {
        private readonly Variable lhs;
        private readonly Variable rhs;

        /// <summary>
        /// Initialises a new instance of <see cref="VariableSetterFrame" /> that outputs code
        /// to set <paramref name="lhs" /> to the value of <paramref name="rhs"/>.
        /// </summary>
        /// <param name="lhs">The left hand side of the statement.</param>
        /// <param name="rhs">The right hand side of the statement.</param>
        public VariableSetterFrame(Variable lhs, Variable rhs)
        {
            this.lhs = lhs;
            this.rhs = rhs;
        }

        /// <summary>
        /// Initialises a new instance of <see cref="VariableSetterFrame" /> that outputs code
        /// to set <paramref name="lhs" /> to the value of <paramref name="rhs"/>.
        /// </summary>
        /// <param name="lhs">The left hand side of the statement.</param>
        /// <param name="rhs">The right hand side of the statement.</param>
        public VariableSetterFrame(Variable lhs, string rhs)
        {
            this.lhs = lhs;
            this.rhs = new Variable(lhs.VariableType, rhs);
        }

        /// <inheritdoc />
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            writer.Write(ToString());

            next();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{lhs} = {rhs};";
        }
    }
}
