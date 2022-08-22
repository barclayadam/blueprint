using System;
using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    /// <summary>
    /// A <see cref="SyncFrame" /> that will output code to create a variable to the value of another.
    /// </summary>
    public class VariableCreationFrame : SyncFrame
    {
        private readonly Variable _lhs;
        private readonly Variable _rhs;

        /// <summary>
        /// Initialises a new instance of <see cref="VariableCreationFrame" /> that outputs code
        /// to create a variable of type <paramref name="variableType" /> to the value of <paramref name="rhs"/>.
        /// </summary>
        /// <param name="variableType">The type of the variable to be created.</param>
        /// <param name="rhs">The right hand side of the statement.</param>
        public VariableCreationFrame(Type variableType, string rhs)
            : this(variableType, Variable.DefaultName(variableType), rhs)
        {
        }

        /// <summary>
        /// Initialises a new instance of <see cref="VariableCreationFrame" /> that outputs code
        /// to create a variable of type <paramref name="variableType" /> to the value of <paramref name="rhs"/>.
        /// </summary>
        /// <param name="variableType">The type of the variable to be created.</param>
        /// <param name="variableName">The name of this variable.</param>
        /// <param name="rhs">The right hand side of the statement.</param>
        public VariableCreationFrame(Type variableType, string variableName, string rhs)
        {
            this._lhs = new Variable(variableType, variableName);
            this._rhs = new Variable(variableType, rhs);
        }

        /// <summary>
        /// The variable that has been created by this <see cref="VariableCreationFrame" />.
        /// </summary>
        public Variable CreatedVariable => this._lhs;

        /// <inheritdoc />
        protected override void Generate(IMethodVariables variables, GeneratedMethod method, IMethodSourceWriter writer, Action next)
        {
            writer.WriteLine(this.ToString());

            next();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"var {this._lhs} = {this._rhs};";
        }
    }
}
