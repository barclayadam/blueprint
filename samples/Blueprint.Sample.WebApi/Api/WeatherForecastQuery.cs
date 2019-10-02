using System;
using System.Linq;
using System.Threading.Tasks;
using Blueprint.Api;

namespace Blueprint.Sample.WebApi.Api
{
    [RootLink("forecast")]
    public class WeatherForecastQuery : IQuery
    {
        public string City { get; set; }
    }

    public class WeatherForecastQueryHandler : IApiOperationHandler<WeatherForecastQuery>
    {
        private static readonly string[] Summaries = new[] {"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"};

        public async Task<object> Invoke(WeatherForecastQuery operation, ApiOperationContext apiOperationContext)
        {
            var rng = new Random();

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    City = operation.City,
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })
                .ToArray();
        }
    }
}
