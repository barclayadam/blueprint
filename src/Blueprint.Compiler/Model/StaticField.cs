using System;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler.Model
{
    /// <summary>
    /// A <c>static</c> field of a <see cref="GeneratedType" />.
    /// </summary>
    public class StaticField : Variable
    {
        private readonly string _initializer;

        /// <summary>
        /// Initialises a new instance of the <see cref="StaticField"/> class.
        /// </summary>
        /// <param name="fieldType">The type of this field.</param>
        /// <param name="name">The name of this field.</param>
        /// <param name="initializer">An initializer statement.</param>
        public StaticField(Type fieldType, string name, string initializer)
            : base(fieldType, "_" + name)
        {
            this._initializer = initializer;
        }

        /// <summary>
        /// Writes the declaration of this static field.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        public void WriteDeclaration(ISourceWriter writer)
        {
            writer.WriteLine($"private static readonly {this.VariableType.FullNameInCode()} {this.Usage} = {this._initializer};");
        }
    }
}
