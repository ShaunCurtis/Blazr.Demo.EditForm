
using Blazr.Core;
/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private IWeatherForecastDataBroker weatherForecastDataBroker;

        public WeatherForecastController(IWeatherForecastDataBroker weatherForecastDataBroker)
            => this.weatherForecastDataBroker = weatherForecastDataBroker;

        [Route("/api/weatherforecast/list")]
        [HttpGet]
        public async Task<ListQueryResult<WeatherForecast>> GetForecastsAsync()
            => await weatherForecastDataBroker.GetWeatherForecastsAsync();

        [Route("/api/weatherforecast/get")]
        [HttpPost]
        public async Task<ItemQueryResult<WeatherForecast>> GetForecastAsync([FromBody] Guid Id)
            => await weatherForecastDataBroker.GetForecastAsync(Id);

        [Route("/api/weatherforecast/add")]
        [HttpPost]
        public async Task<CommandResult> AddRecordAsync([FromBody] WeatherForecast record)
            => await weatherForecastDataBroker.AddForecastAsync(record);

        [Route("/api/weatherforecast/update")]
        [HttpPost]
        public async Task<CommandResult> UpdateRecordAsync([FromBody] WeatherForecast record)
            => await weatherForecastDataBroker.UpdateForecastAsync(record);

        [Route("/api/weatherforecast/delete")]
        [HttpPost]
        public async Task<CommandResult> DeleteRecordAsync([FromBody] Guid Id)
            => await weatherForecastDataBroker.DeleteForecastAsync(Id);
    }
}
