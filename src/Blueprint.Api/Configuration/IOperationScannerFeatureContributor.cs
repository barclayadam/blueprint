namespace Blueprint.Api.Configuration
{
    public interface IOperationScannerFeatureContributor
    {
        void Apply(ApiOperationDescriptor descriptor);
    }
}
