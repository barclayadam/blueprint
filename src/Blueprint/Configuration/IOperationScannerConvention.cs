namespace Blueprint.Configuration;

/// <summary>
/// A convention that can be added to <see cref="OperationScanner" /> to contribute to the scanning
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
}
