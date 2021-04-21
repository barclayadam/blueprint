using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Blueprint.Http;
using Blueprint.Sample.WebApi.Data;
using Blueprint.Sample.WebApi.Tasks;
using Blueprint.Tasks;

namespace Blueprint.Sample.WebApi.Api
{
    [RootLink("weather/modify")]
    public class ModifyWeatherCommand : ICommand<ResourceEvent<WeatherForecast>>
    {
        [Required]
        [FromQuery]
        public string City { get; set; }

        [Required]
        [FromQuery]
        public int Amount { get; set; }

        public ResourceEvent<WeatherForecast> Invoke(IWeatherDataSource weatherDataSource)
        {
            return WeatherEvents.Modified.New(weatherDataSource.Get(City).ElementAt(0));
        }
    }

    /// <summary>
    /// The <see cref="ResourceEventDefinition"/>s for <see cref="TenantApiResource" />.
    /// </summary>
    public static class WeatherEvents
    {
        private static readonly ResourceEventDefinitionFactory<WeatherForecast> _factory =
            ResourceEventDefinition.For<WeatherForecast>();

        /// <summary>
        /// Event raised when a new <see cref="TenantApiResource" /> is updated.
        /// </summary>
        public static readonly ResourceEventDefinition<WeatherForecast> Modified = _factory.Updated("modified");
    }
}
