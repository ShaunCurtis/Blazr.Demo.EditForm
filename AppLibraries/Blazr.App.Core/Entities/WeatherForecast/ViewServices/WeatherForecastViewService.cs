﻿/// ============================================================
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

    public DeoWeatherForecast EditModel { get; private set; } = new DeoWeatherForecast();

    public WeatherForecastViewService(IWeatherForecastDataBroker weatherForecastDataBroker, WeatherForecastsViewService weatherForecastsViewService)
    {
        this.weatherForecastDataBroker = weatherForecastDataBroker!;
        this.weatherForecastsViewService = weatherForecastsViewService;
    }

    public async ValueTask GetForecastAsync(Guid Id)
    {
        var result = await weatherForecastDataBroker!.GetForecastAsync(Id);
        if (result.IsSuccess)
            this.Record = result.Item ?? new();

        this.EditModel.Populate(this.Record);
    }

    public async ValueTask AddRecordAsync(WeatherForecast record)
    {
        this.Record = record;
        var result = await weatherForecastDataBroker!.AddForecastAsync(this.Record);
        weatherForecastsViewService.NotifyListChanged(this, EventArgs.Empty);
    }

    public async ValueTask UpdateRecordAsync(WeatherForecast? record = null)
    {
        this.Record = record ?? EditModel.ToDco;
        var result = await weatherForecastDataBroker!.UpdateForecastAsync(this.Record);
        weatherForecastsViewService.NotifyListChanged(this, EventArgs.Empty);
    }

    public async ValueTask DeleteRecordAsync(Guid Id)
    {
        var result = await weatherForecastDataBroker!.DeleteForecastAsync(Id);
        weatherForecastsViewService.NotifyListChanged(this, EventArgs.Empty);
    }
}
