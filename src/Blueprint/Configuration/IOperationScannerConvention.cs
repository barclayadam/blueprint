using System;

namespace Blueprint.Configuration
{
    /// <summary>
    /// A convention that can be added to <see cref="BlueprintApiOperationScanner" /> to contribute to the scanning
    /// process of finding and registering operations with an <see cref="ApiDataModel" />.
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
        /// Gets a value indicating whether the <see cref="Type" /> is a supported operation and should therefore
        /// be included in the <see cref="ApiDataModel" /> that is being built.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Hosts are responsible for registering a convention that indicates what types of operations it supports
        /// and should be included when scanning assemblies (i.e. a background task processor would only care
        /// operations marked with an IBackgroundTask interface, or a HTTP host may only care about operations with a
        /// route / link attached).
        /// </para>
        /// <para>
        /// If multiple conventions are registered, it only takes a single convention to return <c>true</c> to include
        /// that operation.
        /// </para>
        /// </remarks>
        /// <param name="operationType">The type of operation to check.</param>
        /// <returns>Whether the operation type is supported.</returns>
        bool IsSupported(Type operationType);
    }
}
