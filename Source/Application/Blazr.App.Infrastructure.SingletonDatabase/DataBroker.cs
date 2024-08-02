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
                _weatherDataProvider.WeatherForecasts.Add(request.item);
                return new AddCommandResult(true, request.item.Id);

            case DeleteCommandRequest<WeatherForecast>:
                var deleteRecord = _weatherDataProvider.WeatherForecasts.SingleOrDefault(item => item.Id.Equals(request.item.Id));
                if (deleteRecord is not null)
                    _weatherDataProvider.WeatherForecasts.Remove(deleteRecord);
                return new CommandResult(deleteRecord is not null);

            case UpdateCommandRequest<WeatherForecast>:
                var updateRecord = _weatherDataProvider.WeatherForecasts.SingleOrDefault(item => item.Id.Equals(request.item.Id));
                if (updateRecord is not null)
                {
                    _weatherDataProvider.WeatherForecasts.Remove(updateRecord);
                    _weatherDataProvider.WeatherForecasts.Add(request.item);
                }
                return new CommandResult(updateRecord is not null);

            default:
                throw new ArgumentException("Unknown type of command request", nameof(request));
        }
    }
}
