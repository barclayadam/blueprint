namespace Blueprint
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

        /// <summary>
        /// Builds this middleware in to the generated method, as identified and supported by the supplied <see cref="MiddlewareBuilderContext"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will <strong>only</strong> be called in the case that <see cref="Matches"/> returns <c>true</c>, although there is no requirement
        /// this method would actually modify the method.
        /// </para>
        /// <para>
        /// Care must be taken with regards to the <see cref="MiddlewareBuilderContext.IsNested" /> property that indicates whether this method
        /// is being called to generate a child executor or not (i.e. it will be called twice if <see cref="SupportsNestedExecution" /> returns <c>true</c>).
        /// </para>
        /// </remarks>
        /// <param name="context">The context that represents the current operation that is being processed.</param>
        void Build(MiddlewareBuilderContext context);
    }
}
