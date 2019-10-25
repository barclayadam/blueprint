using System.Collections.Generic;

namespace Blueprint.Sample.WebApi.Data
{
    public interface IWeatherDataSource
    {
        IEnumerable<WeatherForecast> Get(string city);
    }
}
