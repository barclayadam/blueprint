using System.Threading.Tasks;
using Blueprint.Api;
using Blueprint.Sample.WebApi.Data;

namespace Blueprint.Sample.WebApi.Api
{
    [RootLink("forecast")]
    public class WeatherForecastQuery : IQuery
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

        public Task<object> Invoke(WeatherForecastQuery operation, ApiOperationContext apiOperationContext)
        {
            return Task.FromResult((object)weatherDataSource.Get(operation.City));
        }
    }
}
