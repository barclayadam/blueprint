namespace Blueprint.Api
{
    public interface IMiddlewareBuilder
    {
        /// <summary>
        /// Indicates whether this <see cref="IMiddlewareBuilder" /> should be applied to the given description.
        /// </summary>
        /// <remarks>
        /// This decision is made separately to allow tracking of what middleware components were actually applied for
        /// any given generated pipeline for diagnostic purposes.
        /// </remarks>
        /// <param name="operation">The operation to check.</param>
        /// <returns>Whether this builder should / could generate any coe for the given <paramref name="operation"/>.</returns>
        bool Matches(ApiOperationDescriptor operation);

        void Build(MiddlewareBuilderContext context);
    }
}
