namespace Blueprint.Compiler.Frames
{
    /// <summary>
    /// Determines the method of callling a constructor.
    /// </summary>
    public enum ConstructorCallMode
    {
        /// <summary>
        /// The construtor result will be assigned to a variable.
        /// </summary>
        Variable,

        /// <summary>
        /// The constructor result will be used as the return value of
        /// the containing method.
        /// </summary>
        ReturnValue,

        /// <summary>
        /// The resulting object will be used as part of a using statement (i.e.
        /// the result would be <code>using (var instance = new [CtorCall] { ... })</code>
        /// </summary>
        UsingNestedVariable,
    }
}
