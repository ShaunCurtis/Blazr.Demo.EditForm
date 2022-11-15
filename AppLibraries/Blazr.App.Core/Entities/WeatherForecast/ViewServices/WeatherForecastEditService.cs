/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

public class WeatherForecastEditService
{
    private readonly IWeatherForecastDataBroker? weatherForecastDataBroker;

    private readonly WeatherForecastsViewService weatherForecastsViewService;

    public WeatherForecast Record { get; private set; } = new WeatherForecast();

    public readonly WeatherForecastEditContext EditContext = new WeatherForecastEditContext(new());

    public WeatherForecastEditService(IWeatherForecastDataBroker weatherForecastDataBroker, WeatherForecastsViewService weatherForecastsViewService)
    {
        this.weatherForecastDataBroker = weatherForecastDataBroker!;
        this.weatherForecastsViewService = weatherForecastsViewService;
    }

    public async ValueTask GetForecastAsync(Guid Id)
    {
       var result = await weatherForecastDataBroker!.GetForecastAsync(Id);
        if (result.IsSuccess)
            this.Record = result.Item ?? new();

        this.EditContext.Load(this.Record);
    }

    public async ValueTask AddRecordAsync(WeatherForecast? record = null)
    {
        this.Record = record ?? this.EditContext.AsNewRecord();
        var result = await weatherForecastDataBroker!.AddForecastAsync(this.Record);
        this.EditContext.Save();
        weatherForecastsViewService.NotifyListChanged(this, EventArgs.Empty);
    }

    public async ValueTask UpdateRecordAsync(WeatherForecast? record = null)
    {
        this.Record = record ?? this.EditContext.AsNewRecord();
        var result = await weatherForecastDataBroker!.UpdateForecastAsync(this.Record);
        this.EditContext.Save();
        weatherForecastsViewService.NotifyListChanged(this, EventArgs.Empty);
    }

    public async ValueTask DeleteRecordAsync(Guid Id)
    {
        var result = await weatherForecastDataBroker!.DeleteForecastAsync(Id);
        weatherForecastsViewService.NotifyListChanged(this, EventArgs.Empty);
    }
}
