using System;
using Blueprint.Compiler.Util;

namespace Blueprint.Compiler.Model
{
    /// <summary>
    /// Represents a .NET property of a class.
    /// </summary>
    public class Property : Variable
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Property" /> class, with a default
        /// name generated from the given variable type.
        /// </summary>
        /// <param name="variableType">The type of this property / variable.</param>
        public Property(Type variableType)
            : base(variableType)
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="Property" /> class, with the given
        /// name and type.
        /// </summary>
        /// <param name="variableType">The type of this property / variable.</param>
        /// <param name="name">The name of this property.</param>
        public Property(Type variableType, string name)
            : base(variableType, name)
        {
        }

        /// <summary>
        /// Writes the declaration of this property to the given source write.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        public virtual void WriteDeclaration(ISourceWriter writer)
        {
            writer.WriteLine($"public {this.VariableType.FullNameInCode()} {this.Usage} {{get; set;}}");
        }
    }
}
