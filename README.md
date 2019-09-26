[![Build Status][build-shield]][build-url]
[![Tests][test-shield]][test-url]
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![Apache 2.0 License][license-shield]][license-url]

### Blueprint

Blueprint provides a framework to create HTTP APIs, background task processors and command line apps that
are built using a simple operation + handler architecture with a pipeline of middlewares that perform
cross-cutting concerns such as auditing, authorisation and error handling.

Blueprint uses runtime code generation using [Rosyln](https://github.com/dotnet/roslyn) to generate efficient executors for each individual 
operation at startup.

[Report Bug](https://github.com/barclayadam/blueprint/issues)
·
[Request Features](https://github.com/barclayadam/blueprint/issues)

## Table of Contents

* [About the Project](#about-the-project)
  * [Built With](#built-with)
* [Getting Started](#getting-started)
  * [Prerequisites](#prerequisites)
  * [Installation](#installation)
* [Usage](#usage)
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
    [RootLink("echoName")]
    public class EchoNameQuery : IQuery
    {
        public string Name { get; set; }
    }

    public class EchoNameQueryHandler : IApiOperationHandler<EchoNameQuery>
    {
        public async Task<object> Invoke(EchoNameQuery operation, ApiOperationContext apiOperationContext)
        {
            return new { operation.Name };
        }
    }
}
````

Blueprint provides a runtime-generated pipeline runner of operations that can be used in multiple contexts, enabling a codebase
to have a homogeneous way of organising command and queries whilst having a common means of adding cross-cutting concerns.

Blueprint builds a class per operation found at runtime, with middleware builders contributing cross-cutting concerns. Because we
build the pipeline dynamically per type builders are able to eliminate unused code per type, remove reflection over property
types and with DI integration potentially eliminate DI calls and replace them with direct constructor calls in some cases.

### Installation

To use Blueprint API in your ASP.Net app:

1. Add Blueprint.Api to your project (note we only publish to Azure Artifacts Feed right now)
```sh
dotnet add package Blueprint.Api -s https://pkgs.dev.azure.com/blueprint-api/Blueprint/_packaging/Blueprint/nuget/v3/index.json
```
2. Add Blueprint.Api to `Startup.ConfigureServices`
```c#
        public void ConfigureServices(IServiceCollection services)
        {
            // MVC Core is currently a requirement
            services.AddMvcCore();

            services.AddBlueprintApi(o =>
            {
                o.WithApplicationName("MyApplication");

                o.UseMiddlewareBuilder<LoggingMiddlewareBuilder>();
                o.UseMiddlewareBuilder<MessagePopulationMiddlewareBuilder>();
                o.UseMiddlewareBuilder<ValidationMiddlewareBuilder>();
                o.UseMiddlewareBuilder<OperationExecutorMiddlewareBuilder>();
                o.UseMiddlewareBuilder<ResourceEventHandlerMiddlewareBuilder>();
                o.UseMiddlewareBuilder<LinkGeneratorMiddlewareBuilder>();
                o.UseMiddlewareBuilder<FormatterMiddlewareBuilder>();

                o.Scan(typeof(Startup).Assembly);
            });
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

## Roadmap

See the [open issues](https://github.com/barclayadam/blueprint/issues) for a list of proposed features (and known issues).

## Contributing

Contributions are what make the open source community such an amazing place to be learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<!-- LICENSE -->
## License

Distributed under the Apache 2.0 License. See `LICENSE.md` for more information.

<!-- CONTACT -->
## Contact

Adam Barclay - [@barclayadam](https://twitter.com/barclayadam)
Project Link: [https://github.com/barclayadam/blueprint](https://github.com/barclayadam/blueprint)


<!-- ACKNOWLEDGEMENTS -->
<!-- TBD -->

<!-- MARKDOWN LINKS & IMAGES -->
[build-shield]: https://img.shields.io/azure-devops/build/blueprint-api/blueprint/1?style=flat-square
[build-url]: https://dev.azure.com/blueprint-api/Blueprint/_build
[test-shield]: https://img.shields.io/azure-devops/tests/blueprint-api/blueprint/1?style=flat-square
[test-url]: https://dev.azure.com/blueprint-api/Blueprint/_build
[contributors-shield]: https://img.shields.io/github/contributors/barclayadam/blueprint.svg?style=flat-square
[contributors-url]: https://github.com/barclayadam/blueprint/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/barclayadam/blueprint.svg?style=flat-square
[forks-url]: https://github.com/barclayadam/blueprint/network/members
[stars-shield]: https://img.shields.io/github/stars/barclayadam/blueprint.svg?style=flat-square
[stars-url]: https://github.com/barclayadam/blueprint/stargazers
[issues-shield]: https://img.shields.io/github/issues/barclayadam/blueprint.svg?style=flat-square
[issues-url]: https://github.com/barclayadam/blueprint/issues
[license-shield]: https://img.shields.io/github/license/barclayadam/blueprint.svg?style=flat-square
[license-url]: https://github.com/barclayadam/blueprint/blob/master/LICENSE.md
[product-screenshot]: images/web-api-sample.png