﻿/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

/// <summary>
/// The data broker interface abstracts the interface between the logic layer and the data layer.
/// </summary>
public interface IWeatherForecastDataBroker
{
    public ValueTask<bool> AddForecastAsync(WeatherForecast record);

    public ValueTask<bool> UpdateForecastAsync(WeatherForecast record);

    public ValueTask<WeatherForecast> GetForecastAsync(Guid Id);

    public ValueTask<bool> DeleteForecastAsync(Guid Id);

    public ValueTask<IEnumerable<WeatherForecast>> GetWeatherForecastsAsync();
}
