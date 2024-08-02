/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

public interface IDataBroker<TId, TRecord>
{
    public ValueTask<ItemQueryResult<TRecord>> GetItemAsync(ItemQueryRequest<TId> request);
    public ValueTask<ListQueryResult<TRecord>> GetItemsAsync(ListQueryRequest request);
    public ValueTask<ICommandResult> ExecuteCommandAsync(ICommandRequest<WeatherForecast> request);
}
