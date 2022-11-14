/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

public class WeatherForecastViewService
{
    private readonly IWeatherForecastDataBroker? weatherForecastDataBroker;

    private readonly WeatherForecastsViewService weatherForecastsViewService;

    public WeatherForecast Record { get; private set; } = new WeatherForecast();

    public DeoWeatherForecast EditModel { get; private set; }  = new DeoWeatherForecast();

    public WeatherForecastViewService(IWeatherForecastDataBroker weatherForecastDataBroker, WeatherForecastsViewService weatherForecastsViewService)
    {
        this.weatherForecastDataBroker = weatherForecastDataBroker!;
        this.weatherForecastsViewService = weatherForecastsViewService;
    }

    public async ValueTask GetForecastAsync(Guid Id)
    {
       this.Record = await weatherForecastDataBroker!.GetForecastAsync(Id);
        this.EditModel.Populate(this.Record);
    }

    public async ValueTask AddRecordAsync(WeatherForecast record)
    {
        this.Record = record;
        await weatherForecastDataBroker!.AddForecastAsync(this.Record);
        weatherForecastsViewService.NotifyListChanged(this, EventArgs.Empty);
    }

    public async ValueTask UpdateRecordAsync(WeatherForecast? record = null)
    {
        this.Record = record ?? EditModel.ToDco;
        await weatherForecastDataBroker!.UpdateForecastAsync(this.Record);
        weatherForecastsViewService.NotifyListChanged(this, EventArgs.Empty);
    }

    public async ValueTask DeleteRecordAsync(Guid Id)
    {
        _ = await weatherForecastDataBroker!.DeleteForecastAsync(Id);
        weatherForecastsViewService.NotifyListChanged(this, EventArgs.Empty);
    }
}
