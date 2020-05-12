using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Blueprint.Api;
using Blueprint.Api.Http;
using Blueprint.Sample.WebApi.Data;

namespace Blueprint.Sample.WebApi.Api
{
    [RootLink("forecast-inline")]
    public class WeatherForecastInlineQuery : IQuery
    {
        [Required]
        public string City { get; set; }

        public string[] Days { get; set; }

        [FromHeader("X-Header-Key")]
        public string MyHeader { get; set; }

        [FromCookie]
        public string MyCookie { get; set; }

        [FromCookie("a-different-cookie-name")]
        public int MyCookieNumber { get; set; }

        public OkResult<IEnumerable<WeatherForecast>> Invoke(IWeatherDataSource weatherDataSource)
        {
            return new OkResult<IEnumerable<WeatherForecast>>(weatherDataSource.Get(City));
        }
    }
}
