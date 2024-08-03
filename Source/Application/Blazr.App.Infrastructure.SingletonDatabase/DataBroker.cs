/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

using Blazr.App.Core;

namespace Blazr.App.Infrastructure.SingletonDatabase;

public class WeatherDataBroker : IDataBroker<WeatherForecastId, WeatherForecast>
{
    private readonly WeatherDataProvider _weatherDataProvider;

    public WeatherDataBroker(WeatherDataProvider weatherDataProvider)
    {
        _weatherDataProvider = weatherDataProvider;
    }

    public async ValueTask<ItemQueryResult<WeatherForecast>> GetItemAsync(ItemQueryRequest<WeatherForecastId> request)
    {
        // Fake an async call by yielding control.
        await Task.Yield();

        var result = _weatherDataProvider.WeatherForecasts.SingleOrDefault(item => item.Id.Equals(request.id));
        return new ItemQueryResult<WeatherForecast>(result is not null, result);
    }

    public async ValueTask<ListQueryResult<WeatherForecast>> GetItemsAsync(ListQueryRequest request)
    {
        // Fake an async call by yielding control.
        await Task.Yield();

        var count = _weatherDataProvider.WeatherForecasts.Count();
        var result = _weatherDataProvider.WeatherForecasts
            .Skip(request.StartIndex)
            .Take(request.PageSize);
        return new ListQueryResult<WeatherForecast>(result is not null, result ?? Enumerable.Empty<WeatherForecast>(), count);
    }

    public async ValueTask<ICommandResult> ExecuteCommandAsync(ICommandRequest<WeatherForecast> request)
    {
        // Fake an async call by yielding control.
        await Task.Yield();

        switch (request)
        {
            case AddCommandRequest<WeatherForecast>:
                var addResult = await _weatherDataProvider.AddWeatherForecastAsync(request.item);
                return new AddCommandResult(addResult, request.item.Id, addResult ? "Record already exists so not added.": null);

            case DeleteCommandRequest<WeatherForecast>:
                var deleteResult = await _weatherDataProvider.DeleteWeatherForecastAsync(request.item);
                return new CommandResult(deleteResult, deleteResult ? "Record Doesn't exist." : null);

            case UpdateCommandRequest<WeatherForecast>:
                var updateResult = await _weatherDataProvider.UpdateWeatherForecastAsync(request.item);
                return new CommandResult(updateResult, updateResult ? "Record doesn't exist so can't be updated." : null);

            default:
                throw new ArgumentException("Unknown type of command request", nameof(request));
        }
    }
}
