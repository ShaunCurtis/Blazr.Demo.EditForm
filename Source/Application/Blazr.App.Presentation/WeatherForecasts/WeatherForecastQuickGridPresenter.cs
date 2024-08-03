/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

using Blazr.App.Core;
using Microsoft.AspNetCore.Components.QuickGrid;

namespace Blazr.App.Presentation;

public class WeatherForecastQuickGridPresenter
{
    private IDataBroker<WeatherForecastId, WeatherForecast> _dataBroker;

    public WeatherForecastQuickGridPresenter(IDataBroker<WeatherForecastId, WeatherForecast> dataBroker)
    {
        _dataBroker = dataBroker;
    }

    public async ValueTask<GridItemsProviderResult<WeatherForecast>> GetItemsAsync(GridItemsProviderRequest<WeatherForecast> request)
    {
        await Task.Yield();

        var result = await _dataBroker.GetItemsAsync(new ListQueryRequest(request.StartIndex, request.Count ?? 1000));
        return new() { Items = result.Items.ToList(), TotalItemCount = result.TotalCount };
    }
}
