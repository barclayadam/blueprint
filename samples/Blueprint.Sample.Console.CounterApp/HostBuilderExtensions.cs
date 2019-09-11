using System;
using Microsoft.Extensions.Hosting;

namespace Blueprint.Sample.Console.CounterApp
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseDotNetCore2Environment(this IHostBuilder builder)
        {
            // This will work out-of-the-box in .NET Core 3.0 (https://github.com/aspnet/AspNetCore/issues/4150)
            var environmentVariable = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

            return builder.UseEnvironment(environmentVariable ?? EnvironmentName.Production);
        }
    }
}
