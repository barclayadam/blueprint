﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Blueprint.Authorisation;
using Blueprint.Http;
using Blueprint.Sample.WebApi.Data;

namespace Blueprint.Sample.WebApi.Api;

[RootLink("forecast-inline")]
[AllowAnonymous]
public class WeatherForecastInlineQuery : IQuery<IEnumerable<WeatherForecast>>
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

    [FromCookie("a-different-cookie-name-two")]
    public int MyCookieNumberTwo { get; set; }

    public IEnumerable<WeatherForecast> Invoke(IWeatherDataSource weatherDataSource)
    {
        return weatherDataSource.Get(City);
    }
}