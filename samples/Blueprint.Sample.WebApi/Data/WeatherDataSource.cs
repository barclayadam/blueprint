using System;
using System.Collections.Generic;

namespace Blueprint.Sample.WebApi.Data
{
    public class WeatherDataSource : IWeatherDataSource
    {
        private static readonly Random rng = new Random();
        private static readonly string[] summaries = {"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"};

        public IEnumerable<WeatherForecast> Get(string city)
        {
            static WeatherForecast Random(string city, int index)
            {
                return new WeatherForecast
                {
                    City = city,
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = summaries[rng.Next(summaries.Length)],
                };
            }

            return new []
            {
                Random(city, 1),
                Random(city, 2),
                Random(city, 3),
                Random(city, 4),
                Random(city, 5),
            };
        }
    }
}
