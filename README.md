[![Build Status][build-shield]][build-url]
[![Tests][test-shield]][test-url]
[![Coverage][coverage-shield]][coverage-url]
[![Quality][quality-shield]][quality-url]
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![Apache 2.0 License][license-shield]][license-url]

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
    [RootLink("echoName")]
    public class EchoNameQuery : IQuery
    {
        [Required]
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

Blueprint provides a runtime-compiled pipeline runner of operations that can be used in multiple contexts, enabling a codebase
to have a homogeneous way of organising command and queries whilst having a common means of adding cross-cutting concerns.

Blueprint compiles a class per operation, with middleware builders contributing cross-cutting concerns. Because we
build the pipeline dynamically per type builders are able to eliminate unused code per type, remove reflection over property
types and with DI integration potentially eliminate DI calls and replace them with direct constructor calls in some cases.

### Built With

 * [Roslyn](https://github.com/dotnet/roslyn) - For runtime code compilation
 * [Newtonsoft.Json](https://www.newtonsoft.com/json) - For JSON handling

## Installation

To use Blueprint API in your ASP.NET app:

 1. Add our Azure DevOps NuGet feed by adding the source `https://pkgs.dev.azure.com/blueprint-api/Blueprint/_packaging/Blueprint/nuget/v3/index.json`
   to a `NuGet.config` file at your solution root folder:
    ````xml
    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
      <packageSources>
        <add key="blueprint-api" value="https://pkgs.dev.azure.com/blueprint-api/Blueprint/_packaging/Blueprint/nuget/v3/index.json" protocolVersion="3" />
      </packageSources>
    </configuration>
    ````   
 2. Add Blueprint.Api to your project (note we only currently have pre-release NuGet packages)
    ```sh
    dotnet add package Blueprint.Api -v 0.1.0-*
    ```
 3. Add Blueprint.Api to `Startup.ConfigureServices`
    ```c#
            public void ConfigureServices(IServiceCollection services)
            {
                // MVC Core is currently a requirement
                services.AddMvcCore();
    
                services.AddApplicationInsightsTelemetry();
    
                services.AddBlueprintApi(o => o
                    .SetApplicationName("SampleWebApi")
                    .ScanForOperations(typeof(Startup).Assembly)
                    .Pipeline(m => m
                        .AddLogging()
                        .AddApplicationInsights()
                        .AddHttp()
                        .AddValidation()
                        .AddHateoasLinks()
                        .AddResourceEvents()
                    ));
            }
    ```
4. Add Blueprint.Api to `Startup.Configure`
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

The pipeline is fully customisable with many built-in middlewares such as APM integration, logging, auditing, link (HATEOAS) 
generation and validation. 

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
[build-shield]: https://img.shields.io/azure-devops/build/blueprint-api/blueprint/1?style=flat-square
[build-url]: https://dev.azure.com/blueprint-api/Blueprint/_build
[test-shield]: https://img.shields.io/azure-devops/tests/blueprint-api/blueprint/1?style=flat-square
[test-url]: https://dev.azure.com/blueprint-api/Blueprint/_build
[coverage-shield]: https://img.shields.io/azure-devops/coverage/blueprint-api/blueprint/1?style=flat-square
[coverage-url]: https://dev.azure.com/blueprint-api/Blueprint/_build
[quality-shield]: https://sonarcloud.io/api/project_badges/measure?project=barclayadam_blueprint&metric=alert_status
[quality-url]: https://sonarcloud.io/dashboard?id=barclayadam_blueprint
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
