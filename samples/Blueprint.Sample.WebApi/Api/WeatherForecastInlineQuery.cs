using System.Collections.Generic;
using Blueprint.Api;
using Blueprint.Api.Http;
using Blueprint.Sample.WebApi.Data;

namespace Blueprint.Sample.WebApi.Api
{
    [RootLink("forecast-inline")]
    public class WeatherForecastInlineQuery : IQuery
    {
        public string City { get; set; }

        public OkResult<IEnumerable<WeatherForecast>> Invoke(IWeatherDataSource weatherDataSource)
        {
            return new OkResult<IEnumerable<WeatherForecast>>(weatherDataSource.Get(City));
        }
    }
}
