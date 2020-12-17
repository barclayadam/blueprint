using Blueprint.Http.Formatters;
using Microsoft.Extensions.Options;

namespace Blueprint.Http
{
    internal class BlueprintHttpOptionsSetup : IConfigureOptions<BlueprintHttpOptions>
    {
        private readonly IOptions<BlueprintJsonOptions> _jsonOptions;

        public BlueprintHttpOptionsSetup(IOptions<BlueprintJsonOptions> jsonOptions)
        {
            this._jsonOptions = jsonOptions;
        }

        public void Configure(BlueprintHttpOptions options)
        {
            var jsonSerializerOptions = this._jsonOptions.Value.SerializerOptions;

            options.OutputFormatters.Add(new SystemTextJsonResultOutputFormatter(jsonSerializerOptions));
            options.BodyParsers.Add(new SystemTextJsonBodyParser(jsonSerializerOptions));

            options.BodyParsers.Add(new FormBodyParser());
        }
    }
}
