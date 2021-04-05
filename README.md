[![Build Status][build-shield]][build-url]
[![Coverage][coverage-shield]][coverage-url]
[![Quality][quality-shield]][quality-url]
[![Contributors][contributors-shield]][contributors-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![Apache 2.0 License][license-shield]][license-url]
[![Gitpod][gitpod-shield]][gitpod-url]

### Blueprint

Blueprint provides a framework to create HTTP APIs, background task processors and command line apps that
are built using a simple operation + handler architecture with a pipeline of middlewares that perform
cross-cutting concerns such as auditing, authorisation and error handling.

Blueprint uses runtime code compilation using [Rosyln](https://github.com/dotnet/roslyn) to generate efficient executors for each individual 
operation at startup.

[Report Bug](https://github.com/barclayadam/blueprint/issues)
·
[Request Features](https://github.com/barclayadam/blueprint/issues)

## Table of Contents

* [About the Project](#about-the-project)
  * [Built With](#built-with)
* [Installation](#installation)
* [Roadmap](#roadmap)
* [Contributing](#contributing)
* [License](#license)
* [Contact](#contact)
* [Acknowledgements](#acknowledgements)

<!-- ABOUT THE PROJECT -->
## About The Project

````c#
namespace Blueprint.Sample.WebApi.Api
{
    [RootLink("forecast")]
    public class WeatherForecastQuery : IQuery<IEnumerable<WeatherForecast>>
    {
        [Required]
        public string City { get; set; }

        [FromHeader("X-Header-Key")]
        public string MyHeader { get; set; }

        [FromCookie]
        public string MyCookie { get; set; }

        [FromCookie("a-different-cookie-name")]
        public int MyCookieNumber { get; set; }

        public IEnumerable<WeatherForecast> Invoke(IWeatherDataSource weatherDataSource)
        {
            return weatherDataSource.Get(this.City);
        }
    }
}
````

Blueprint provides a runtime-compiled pipeline runner of operations that can be used in multiple contexts, enabling a codebase
to have a homogeneous way of organising command and queries whilst having a common means of adding cross-cutting concerns.

Blueprint compiles a class per operation, with middleware builders contributing cross-cutting concerns. Because we
build the pipeline dynamically per type builders are able to eliminate unused code per type, remove reflection over property
types and with DI integration potentially eliminate DI calls and replace them with direct constructor calls in some cases.

Given the `WeatherForecastQuery` class above Blueprint will generate a class similar to the one below that will
execute the query when the `/forecast?city=London` URL is hit.

````c#
public class WeatherForecastQueryExecutorPipeline : Blueprint.Api.IOperationExecutorPipeline
{
    private readonly Microsoft.Extensions.Logging.ILogger _logger;
    private readonly Blueprint.Sample.WebApi.Data.IWeatherDataSource _weatherDataSource;
    private readonly Blueprint.Api.IApiLinkGenerator _apiLinkGenerator;
    private readonly Blueprint.Core.Errors.IErrorLogger _errorLogger;

    public WeatherForecastQueryExecutorPipeline(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, Blueprint.Sample.WebApi.Data.IWeatherDataSource weatherDataSource, Blueprint.Api.IApiLinkGenerator apiLinkGenerator, Blueprint.Core.Errors.IErrorLogger errorLogger)
    {
        _logger = loggerFactory.CreateLogger("Blueprint.Sample.WebApi.Api.WeatherForecastQueryExecutorPipeline");
        _weatherDataSource = weatherDataSource;
        _apiLinkGenerator = apiLinkGenerator;
        _errorLogger = errorLogger;
    }

    public async System.Threading.Tasks.Task<Blueprint.Api.OperationResult> ExecuteAsync(Blueprint.Api.ApiOperationContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var weatherForecastQuery = (Blueprint.Sample.WebApi.Api.WeatherForecastQuery) context.Operation;
        var httpContext = Blueprint.Api.Http.ApiOperationContextHttpExtensions.GetHttpContext(context);
        var requestTelemetry = httpContext.Features.Get<Microsoft.ApplicationInsights.DataContracts.RequestTelemetry>();

        try
        {
            // ApplicationInsightsMiddleware
            if (requestTelemetry != null)
            {
                requestTelemetry.Name = "WeatherForecastInline";
            }

            // MessagePopulationMiddlewareBuilder
             var fromCookieMyCookie = httpContext.Request.Cookies["MyCookie"];
            weatherForecastQuery.MyCookie = fromCookieMyCookie != null ? fromCookieMyCookie.ToString() : weatherForecastQuery.MyCookie;

            var fromCookieMyCookieNumber = httpContext.Request.Cookies["a-different-cookie-name"];
            weatherForecastQuery.MyCookieNumber = fromCookieMyCookieNumber != null ? int.Parse(fromCookieMyCookieNumber.ToString()) : weatherForecastQuery.MyCookieNumber;

            var fromHeaderMyHeader = httpContext.Request.Headers["X-Header-Key"];
            weatherForecastQuery.MyHeader = fromHeaderMyHeader != Microsoft.Extensions.Primitives.StringValues.Empty ? fromHeaderMyHeader.ToString() : weatherForecastQuery.MyHeader;

            var fromQueryCity = httpContext.Request.Query["City"];
            weatherForecastQuery.City = fromQueryCity != Microsoft.Extensions.Primitives.StringValues.Empty ? fromQueryCity.ToString() : weatherForecastQuery.City;

            var fromQueryDays = httpContext.Request.Query["Days"] == Microsoft.Extensions.Primitives.StringValues.Empty ? httpContext.Request.Query["Days[]"] : httpContext.Request.Query["Days"];
            weatherForecastQuery.Days = fromQueryDays != Microsoft.Extensions.Primitives.StringValues.Empty ? (System.String[]) Blueprint.Api.Http.MessagePopulation.HttpPartMessagePopulationSource.ConvertValue("Days", fromQueryDays, typeof(System.String[])) : weatherForecastQuery.Days;

            // ValidationMiddlewareBuilder
            var validationFailures = new Blueprint.Api.Validation.ValidationFailures();
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(weatherForecastQuery);
            validationContext.MemberName = "City";
            validationContext.DisplayName = "City";

            // context.Descriptor.Properties[0] == WeatherForecastQuery.City
            foreach (var attribute in context.Descriptor.PropertyAttributes[0])
            {
                if (attribute is System.ComponentModel.DataAnnotations.ValidationAttribute x)
                {
                    var result =  x.GetValidationResult(weatherForecastQuery.City, validationContext);
                    if (result != System.ComponentModel.DataAnnotations.ValidationResult.Success)
                    {
                        validationFailures.AddFailure(result);
                    }
                }
            }

            if (validationFailures.Count > 0)
            {
                var validationFailedOperationResult = new Blueprint.Api.Middleware.ValidationFailedOperationResult(validationFailures);
                return validationFailedOperationResult;
            }

            // OperationExecutorMiddlewareBuilder
            _logger.Log(Microsoft.Extensions.Logging.LogLevel.Debug, "Executing API operation. handler_type=WeatherForecastQuery");
            var handlerResult = weatherForecastQuery.Invoke(_weatherDataSource);
            Blueprint.Api.OperationResult operationResult = handlerResult;

            // LinkGeneratorMiddlewareBuilder
            var resourceLinkGeneratorIEnumerable = context.ServiceProvider.GetRequiredService<System.Collections.Generic.IEnumerable<Blueprint.Api.IResourceLinkGenerator>>();
            await Blueprint.Api.Middleware.LinkGeneratorHandler.AddLinksAsync(_apiLinkGenerator, resourceLinkGeneratorIEnumerable, context, operationResult);

            // BackgroundTaskRunnerMiddleware
            var backgroundTaskScheduler = context.ServiceProvider.GetRequiredService<Blueprint.Tasks.IBackgroundTaskScheduler>();
            await backgroundTaskScheduler.RunNowAsync();

            // ReturnFrameMiddlewareBuilder
            return operationResult;
        }
        catch (Blueprint.Api.Validation.ValidationException e)
        {
            var validationFailedOperationResult = new Blueprint.Api.Middleware.ValidationFailedOperationResult(e.ValidationResults);
            return validationFailedOperationResult;
        }
        catch (System.ComponentModel.DataAnnotations.ValidationException e)
        {
            var validationFailedOperationResult = Blueprint.Api.Middleware.ValidationMiddlewareBuilder.ToValidationFailedOperationResult(e);
            return validationFailedOperationResult;
        }
        catch (System.Exception e)
        {
            var userAuthorisationContext = context.UserAuthorisationContext;
            var identifier = new Blueprint.Core.Authorisation.UserExceptionIdentifier(userAuthorisationContext);

            userAuthorisationContext?.PopulateMetadata((k, v) => e.Data[k] = v?.ToString());
            e.Data["WeatherForecastQuery.City"] = weatherForecastQuery.City?.ToString();
            e.Data["WeatherForecastQuery.MyHeader"] = weatherForecastQuery.MyHeader?.ToString();
            e.Data["WeatherForecastQuery.MyCookie"] = weatherForecastQuery.MyCookie?.ToString();
            e.Data["WeatherForecastQuery.MyCookieNumber"] = weatherForecastQuery.MyCookieNumber.ToString();

            _errorLogger.Log(e, identifier);

            if (requestTelemetry != null)
            {
                requestTelemetry.Success = false;
            }

            return new Blueprint.Api.UnhandledExceptionOperationResult(e);
        }
        finally
        {
            var userContext = context.UserAuthorisationContext;
            if (requestTelemetry != null)
            {
                if (userContext != null && userContext.IsAnonymous == false)
                {
                    requestTelemetry.Context.User.AuthenticatedUserId = userContext.Id;
                    requestTelemetry.Context.User.AccountId = userContext.AccountId;
                }

                requestTelemetry.Success = requestTelemetry.Success ?? true;
            }

            stopwatch.Stop();
            _logger.Log(Microsoft.Extensions.Logging.LogLevel.Information, "Operation {0} finished in {1}ms", "Blueprint.Sample.WebApi.Api.WeatherForecastQuery", stopwatch.Elapsed.TotalMilliseconds);
        }
    }
}
````

### Built With

 * [Roslyn](https://github.com/dotnet/roslyn) - For runtime code compilation
 * [Newtonsoft.Json](https://www.newtonsoft.com/json) - For JSON handling

## Installation

To use Blueprint API in your ASP.NET app:

 1.  Add Blueprint.Api to your project
    ```sh
    dotnet add package Blueprint.Api
    dotnet add package Blueprint.Api.Http 
    ```
 2. Add Blueprint.Api to `Startup.ConfigureServices`
    ```c#
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddApplicationInsightsTelemetry();
    
                services.AddBlueprintApi(b => b
                    .SetApplicationName("SampleWebApi")
                    .Operations(o => o.ScanForOperations(typeof(Startup).Assembly))
                    .AddHttp()
                    .AddApplicationInsights()
                    .Pipeline(m => m
                        .AddLogging()
                        .AddValidation()
                        .AddHateoasLinks()
                    ));
            }
    ```
3. Add Blueprint.Api to `Startup.Configure`
    ```c#
            public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
            {
                app.UseForwardedHeaders();
    
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler();
                }
    
                app.UseBlueprintApi("api/");
            }
    ```
   
### Compilation

At runtime Blueprint will, for every operation it finds, generate a class that is used for running an operation with
all the configured middlewares.

The pipeline is fully customisable with many built-in middlewares such as APM integration, logging, auditing, link
generation (HATEOAS) and validation. 

## Roadmap

See the [open issues](https://github.com/barclayadam/blueprint/issues) for a list of proposed features (and known issues).

## Contributing

Contributions are what make the open source community such an amazing place to be learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

Distributed under the Apache 2.0 License. See `LICENSE.md` for more information.

## Contact

Adam Barclay - [@barclayadam](https://twitter.com/barclayadam)

## Acknowledgements

 * [LamarCompiler/LamarCodeGeneration](https://github.com/JasperFx/lamar/tree/master/src/LamarCodeGeneration) Blueprint.Compiler is a modified version of LamarCompiler that
 was present in Lamar before being changed to using Expressions for compilation

<!-- MARKDOWN LINKS & IMAGES -->
[build-shield]: https://img.shields.io/github/workflow/status/barclayadam/blueprint/Blueprint%20Build?style=flat
[build-url]: https://dev.azure.com/blueprint-api/Blueprint/_build
[coverage-shield]: https://img.shields.io/sonar/coverage/barclayadam_blueprint?server=https%3A%2F%2Fsonarcloud.io
[coverage-url]: https://sonarcloud.io/component_measures?id=barclayadam_blueprint&metric=Coverage
[quality-shield]: https://sonarcloud.io/api/project_badges/measure?project=barclayadam_blueprint&metric=alert_status
[quality-url]: https://sonarcloud.io/dashboard?id=barclayadam_blueprint
[contributors-shield]: https://img.shields.io/github/contributors/barclayadam/blueprint.svg?style=flat
[contributors-url]: https://github.com/barclayadam/blueprint/graphs/contributors
[stars-shield]: https://img.shields.io/github/stars/barclayadam/blueprint.svg?style=flat
[stars-url]: https://github.com/barclayadam/blueprint/stargazers
[issues-shield]: https://img.shields.io/github/issues/barclayadam/blueprint.svg?style=flat
[issues-url]: https://github.com/barclayadam/blueprint/issues
[license-shield]: https://img.shields.io/github/license/barclayadam/blueprint.svg?style=flat
[license-url]: https://github.com/barclayadam/blueprint/blob/master/LICENSE.md
[gitpod-url]: https://gitpod.io/#https://github.com/barclayadam/blueprint
[gitpod-shield]: https://img.shields.io/badge/Gitpod-ready--to--code-blue?logo=gitpod?style=flat
