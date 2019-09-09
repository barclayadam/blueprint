using Blueprint.Compiler.Model;

namespace Blueprint.Compiler.Frames
{
    /// <summary>
    /// An <see cref="IfBlock" /> that has will check whether the given variable is <c>null</c>, executing the inner
    /// frames if it is <strong>NOT</strong>.
    /// </summary>
    public class IfNotNullBlock : IfBlock
    {
        /// <summary>
        /// Initializes a new instance of <see cref="IfNotNullBlock" /> that will execute the inner frames in the case
        /// of the <paramref name="variable"/> being null at runtime.
        /// </summary>
        /// <param name="variable">The variable to check.</param>
        /// <param name="inner">The inner frames that will be executed in the case the variable is not null.</param>
        public IfNotNullBlock(Variable variable, params Frame[] inner) : base($"{variable} != null", inner)
        {
        }
    }
}
