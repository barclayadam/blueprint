using Blueprint.Http.Formatters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Blueprint.Http
{
    internal class BlueprintHttpOptionsSetup : IConfigureOptions<BlueprintHttpOptions>
    {
        private readonly IOptions<BlueprintJsonOptions> _jsonOptions;
        private readonly IHostEnvironment _hostEnvironment;

        public BlueprintHttpOptionsSetup(IOptions<BlueprintJsonOptions> jsonOptions, IHostEnvironment hostEnvironment)
        {
            this._jsonOptions = jsonOptions;
            this._hostEnvironment = hostEnvironment;
        }

        public void Configure(BlueprintHttpOptions options)
        {
            var jsonSerializerOptions = this._jsonOptions.Value.SerializerOptions;

            options.OutputFormatters.Add(new SystemTextJsonResultOutputFormatter(jsonSerializerOptions));
            options.BodyParsers.Add(new SystemTextJsonBodyParser(jsonSerializerOptions));

            options.BodyParsers.Add(new FormBodyParser());

            // By default we will only expose the exception details in development environments
            options.ExposeExceptionDetailsInErrorResponses = this._hostEnvironment.IsDevelopment();
        }
    }
}
