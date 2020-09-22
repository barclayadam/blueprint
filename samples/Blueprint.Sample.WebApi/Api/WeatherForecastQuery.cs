using System.Collections.Generic;
using System.Threading.Tasks;
using Blueprint.Sample.WebApi.Data;

namespace Blueprint.Sample.WebApi.Api
{
    [RootLink("forecast")]
    public class WeatherForecastQuery : IQuery<IEnumerable<WeatherForecast>>
    {
        public string City { get; set; }
    }

    public class WeatherForecastQueryHandler : IApiOperationHandler<WeatherForecastQuery>
    {
        private readonly IWeatherDataSource weatherDataSource;

        public WeatherForecastQueryHandler(IWeatherDataSource weatherDataSource)
        {
            this.weatherDataSource = weatherDataSource;
        }

        public ValueTask<object> Handle(WeatherForecastQuery operation, ApiOperationContext apiOperationContext)
        {
            return new ValueTask<object>(weatherDataSource.Get(operation.City));
        }
    }
}
