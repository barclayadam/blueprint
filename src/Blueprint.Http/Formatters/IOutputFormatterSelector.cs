namespace Blueprint.Http.Formatters
{
    /// <summary>
    /// Selects an <see cref="IOperationResultOutputFormatter"/> to write a response to the current request.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default implementation of <see cref="IOperationResultOutputFormatter"/> provided by Blueprint HTTP
    /// is <see cref="DefaultOutputFormatterSelector"/>.
    /// </para>
    /// <para>
    /// The default implementation is controlled by settings on <see cref="BlueprintHttpOptions"/>, most notably:
    /// <see cref="BlueprintHttpOptions.OutputFormatters"/>, <see cref="BlueprintHttpOptions.RespectBrowserAcceptHeader"/>, and
    /// <see cref="BlueprintHttpOptions.ReturnHttpNotAcceptable"/>.
    /// </para>
    /// </remarks>
    public interface IOutputFormatterSelector
    {
        /// <summary>
        /// Selects an <see cref="IOperationResultOutputFormatter"/> to write the response based on the provided values and the current request.
        /// </summary>
        /// <param name="context">The context that contains details on the request and will have <see cref="OutputFormatterCanWriteContext.ContentType" />
        /// set to the chosen content type.</param>
        /// <returns>The selected <see cref="IOperationResultOutputFormatter"/>, or <c>null</c> if one could not be selected.</returns>
        public IOperationResultOutputFormatter SelectFormatter(OutputFormatterCanWriteContext context);
    }
}
