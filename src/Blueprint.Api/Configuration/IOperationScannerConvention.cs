using System;

namespace Blueprint.Api.Configuration
{
    /// <summary>
    /// A convention that can be added to <see cref="BlueprintApiOperationScanner" /> to contribute to the scanning
    /// process of finding and registering <see cref="IApiOperation" />s with an <see cref="ApiDataModel" />.
    /// </summary>
    public interface IOperationScannerConvention
    {
        /// <summary>
        /// Applies the convention of this instance to the constructed <see cref="ApiOperationDescriptor" />, typically
        /// adding new feature data (see <see cref="ApiOperationDescriptor.SetFeatureData" />) or modifying existing
        /// data.
        /// </summary>
        /// <param name="descriptor">The descriptor to modify.</param>
        void Apply(ApiOperationDescriptor descriptor);

        /// <summary>
        /// Gets a value indicating whether the <see cref="Type" /> should be included in the list of
        /// operations that get scanned.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is useful for hosts to register a convention that excludes types that it is not interested in, that
        /// it does not / cannot handle (i.e. a background task processor would only care about marked operations, or
        /// HTTP only cares about operations with a route / link attached).
        /// </para>
        /// <para>
        /// If multiple conventions are registered, it only takes a single convention to return <c>false</c> to exclude
        /// that operation.
        /// </para>
        /// </remarks>
        /// <param name="operationType">The type of operation to check.</param>
        /// <returns>Whether to register the given operation type.</returns>
        bool ShouldInclude(Type operationType);
    }
}
