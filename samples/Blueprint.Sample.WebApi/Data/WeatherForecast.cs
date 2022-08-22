using System;
using JetBrains.Annotations;

namespace Blueprint.Sample.WebApi.Data;

public class WeatherForecast
{
    public string City { get; set; }

    public DateTime Date { get; set; }

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    [CanBeNull] public string Summary { get; set; }
}