using System;
using System.Collections.Generic;
using System.Linq;
using Blueprint.Sample.WebApi.Data;
using Microsoft.AspNetCore.Mvc;

namespace Blueprint.Sample.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IWeatherDataSource weatherDataSource;

        public WeatherForecastController(IWeatherDataSource weatherDataSource)
        {
            this.weatherDataSource = weatherDataSource;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get(string city)
        {
            return weatherDataSource.Get(city);
        }
    }
}
