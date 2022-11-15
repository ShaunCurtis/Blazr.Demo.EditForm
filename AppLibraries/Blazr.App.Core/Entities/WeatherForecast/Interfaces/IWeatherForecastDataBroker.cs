/// ============================================================
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
    public ValueTask<CommandResult> AddForecastAsync(WeatherForecast record);

    public ValueTask<CommandResult> UpdateForecastAsync(WeatherForecast record);

    public ValueTask<ItemQueryResult<WeatherForecast>> GetForecastAsync(Guid Id);

    public ValueTask<CommandResult> DeleteForecastAsync(Guid Id);

    public ValueTask<ListQueryResult<WeatherForecast>> GetWeatherForecastsAsync();
}
