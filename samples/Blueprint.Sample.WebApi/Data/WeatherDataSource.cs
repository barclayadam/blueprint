using System;
using System.Collections.Generic;
using System.Linq;

namespace Blueprint.Sample.WebApi.Data
{
    public class WeatherDataSource : IWeatherDataSource
    {
        private static readonly string[] summaries = {"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"};

        public IEnumerable<WeatherForecast> Get(string city)
        {
            var rng = new Random();

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    City = city,
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = summaries[rng.Next(summaries.Length)],
                })
                .ToArray();
        }
    }
}
